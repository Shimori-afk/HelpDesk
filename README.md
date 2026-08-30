# HelpDesk

A full-stack internal IT support ticketing system — built as a portfolio project to demonstrate
production-style full-stack engineering: a layered ASP.NET Core API backed by PostgreSQL, a
zoneless Angular 22 frontend with Material 3, real-time updates over SignalR, a background SLA
monitor, JWT auth with refresh-token rotation, and a Dockerized deployment.

![Dashboard](docs/screenshots/dashboard.png)

## Table of contents

- [Overview](#overview)
- [Features](#features)
- [Technology stack](#technology-stack)
- [Architecture](#architecture)
- [Database schema](#database-schema)
- [API](#api)
- [Authentication](#authentication)
- [Real-time updates (SignalR)](#real-time-updates-signalr)
- [SLA monitoring & background service](#sla-monitoring--background-service)
- [Running the project](#running-the-project)
- [Test credentials](#test-credentials)
- [Testing](#testing)
- [Screenshots](#screenshots)
- [Future improvements](#future-improvements)

## Overview

Employees raise IT support tickets; agents triage, work, and resolve them; admins manage
users/categories/departments and watch reports. The whole ticket lifecycle — creation, assignment,
status transitions, comments, escalation, SLA breach — is enforced server-side and mirrored to
connected clients in real time.

## Features

**Employee**
- Create tickets with a category, priority, department, and optional location
- Track their own tickets, comment on them, and reopen a resolved ticket if the issue recurs
- Live notification bell for updates on tickets they raised

**Agent**
- Full ticket queue with server-side search, filtering (status/priority/assignment/overdue) and
  pagination
- Assign/reassign tickets, change status through an enforced lifecycle, change priority, escalate
- Live toast when a new ticket comes in, live badge/notification on ticket activity

**Admin**
- Everything an agent can do, plus category/department management and a reports dashboard
- Full visibility across every ticket regardless of who created or is assigned to it

**Everyone**
- Role-scoped dashboard with clickable stat cards that deep-link into a pre-filtered ticket list
- Per-ticket SLA indicator (🟢 within SLA / 🟠 approaching / 🔴 breached), computed server-side
- Full activity timeline per ticket (status changes, assignment, priority changes, comments,
  escalation — including system-generated entries from the SLA monitor)

## Technology stack

| Layer | Stack |
|---|---|
| Frontend | Angular 22 (standalone, zoneless), Angular Material 3, ng2-charts/Chart.js, SignalR client |
| Backend | ASP.NET Core 10 Web API, Entity Framework Core 10, FluentValidation, SignalR |
| Database | PostgreSQL 17 |
| Auth | JWT bearer access tokens + rotating refresh tokens, BCrypt password hashing |
| Testing | xUnit + Moq + EF Core InMemory (backend), Vitest (frontend, via `@angular/build:unit-test`) |
| Infrastructure | Docker Compose (postgres / backend / nginx-served frontend) |

## Architecture

**Backend** — layered, with no business logic in controllers:

```
Controllers/     → HTTP concerns only: model binding, validation trigger, status codes
Services/        → business logic (TicketService, AuthService, NotificationDispatcher, ...)
Data/            → ApplicationDbContext, EF Core entity configurations, hand-authored migrations
BackgroundServices/ → SlaMonitoringService (hosted service)
Common/          → cross-cutting concerns: TicketLifecycle state machine, SlaCalculator,
                   ICurrentUserService, typed AppExceptions
Middleware/      → global exception handling → typed HTTP status codes
Hubs/            → NotificationHub (SignalR)
DTOs/ Validators/ → request/response shapes and FluentValidation rules, one per DTO
```

A request never touches `DbContext` directly from a controller — controllers call into a service
interface, which owns the EF Core queries and the business rules (e.g. `TicketService` is the only
place that knows how a ticket is allowed to change status, and every mutation there runs through
`TicketLifecycle.CanTransition`).

**Frontend** — standalone components, no NgModules:

```
core/
  guards/        → authGuard (must be logged in), roleGuard (must have an allowed role)
  interceptors/  → authInterceptor (attaches JWT, refreshes-and-retries on 401),
                   errorInterceptor (toasts non-401 API errors)
  services/      → one per resource (Ticket, Auth, Dashboard, Report, Signalr, ...), all
                   `providedIn: 'root'`, all using `inject()`
  models/        → TypeScript interfaces mirroring the backend DTOs
features/        → one folder per screen, lazy-loaded via loadComponent/loadChildren
layout/shell/    → authenticated app chrome: nav, notification bell, SignalR lifecycle
```

Every feature route is lazy-loaded; the Reports page additionally lazy-loads `ng2-charts`/Chart.js
via a route-level provider (`reports.routes.ts`) so charting code never enters the eagerly-loaded
main bundle.

## Database schema

PostgreSQL via EF Core, migrations hand-authored (see note below). Core entities:

| Table | Notes |
|---|---|
| `Users` | role (Employee/Agent/Admin), status, optional department |
| `Departments`, `Categories` | lookup tables, unique names |
| `SlaPolicies` | one row per `TicketPriority`, response/resolution time in minutes |
| `Tickets` | status, priority, category/department/creator/assigned-agent FKs, SLA due dates |
| `TicketComments` | ticket + author + message |
| `TicketActivities` | full audit trail per ticket; `UserId` is nullable — null means the SLA background service made the change, not a human |
| `Notifications` | per-user, typed (`TicketAssigned`, `SlaBreached`, ...), read/unread |
| `RefreshTokens` | rotating, revocable |
| `Attachments` | entity + DTOs modeled; upload endpoint not yet wired up (see Future improvements) |

Indexes exist on every FK plus the columns the ticket list filters/sorts by (`Status`, `Priority`,
`CreatedAt`).

> **Why hand-authored migrations?** `dotnet-ef` design-time tooling is blocked by Windows Smart App
> Control (enforced mode) on the machine this was built on — `dotnet build` runs fine, but tooling
> that loads and reflects over the built assembly does not. Rather than disabling a system security
> feature, migrations were hand-written directly (`Data/Migrations/*.cs`) and validated by running
> `Database.MigrateAsync()` against a real Postgres instance in Docker on every phase.

## API

JSON REST API, documented interactively via Swagger UI at `/swagger` (always enabled — this is a
portfolio project, not a production service, so reviewers can explore it). JWT bearer auth on every
route except `POST /api/auth/{register,login,refresh}`.

| Resource | Endpoints |
|---|---|
| Auth | `POST /register`, `POST /login`, `POST /refresh`, `POST /logout`, `GET /me` |
| Tickets | `GET /`, `GET /{id}`, `POST /`, `PUT /{id}`, `DELETE /{id}` (Admin), `GET /{id}/activities`, `POST /{id}/comments`, `PUT /{id}/status`, `PUT /{id}/assign` (Agent/Admin), `PUT /{id}/priority` (Agent/Admin), `POST /{id}/escalate` (Agent/Admin) |
| Categories / Departments | `GET /` (everyone), `POST` / `PUT /{id}` / `DELETE /{id}` (Admin, blocked if still referenced by a ticket) |
| Users | `GET /?role=` (Agent/Admin — powers the assignee picker) |
| Notifications | `GET /`, `PUT /{id}/read`, `PUT /read-all` |
| Dashboard | `GET /summary` — role-scoped stat counts |
| Reports | `GET /summary` (Admin) — 30-day trend, category/priority/status/agent breakdowns, average resolution time, SLA compliance rate |

Ticket list supports server-side `search`, `status`, `priority`, `categoryId`, `departmentId`,
`assignedAgentId`, `unassigned`, `overdue`, `createdFrom`/`createdTo`, `sortBy`/`sortDescending`,
and pagination (`page`/`pageSize`).

Errors are typed and mapped to real HTTP status codes by a global exception-handling middleware —
`404`/`409`/`401`/`403`/`400` for domain exceptions, `422` with a field-error map for FluentValidation
failures, `500` (generic message, full detail only logged server-side) for anything unexpected.

## Authentication

- Passwords hashed with BCrypt; JWT access tokens signed HS256, short-lived
- Refresh tokens are random, stored server-side, and rotated on every use (`RefreshTokens` table);
  logging out revokes the current one
- `POST /register` always creates an `Employee` — role is never trusted from client input
- The Angular `authInterceptor` attaches the access token to every request except the auth-free
  endpoints, and on a `401` transparently calls `/auth/refresh` and retries the original request
  once before giving up and logging out

## Real-time updates (SignalR)

- `NotificationHub` at `/hubs/notifications`, JWT-authenticated over the WebSocket handshake (the
  browser client can't set an `Authorization` header on the handshake, so the token is passed as an
  `access_token` query parameter, accepted only for `/hubs` paths)
- Agents/Admins auto-join an `Agents` group on connect and get a live toast the moment any new
  ticket is created
- Every user gets pushed their own `Notification` rows in real time (assignment, status/priority
  changes, comments, escalation, SLA alerts) via `Clients.User(userId)`
- The Angular `Signalr` service wraps `HubConnectionBuilder` with `withAutomaticReconnect()`; the
  shell subscribes to its signals with `effect()` (not polling) and tears the connection down in
  `ngOnDestroy`

## SLA monitoring & background service

Every ticket priority has a configurable response/resolution SLA window (`SlaPolicies`). SLA state
(`WithinSla` / `ApproachingBreach` / `Breached`) is **always computed server-side** — `SlaCalculator`
— and returned on every ticket DTO, never derived from wall-clock math in the Angular client.

`SlaMonitoringService` is an ASP.NET Core `BackgroundService` that wakes up once a minute, and for
every non-closed ticket:
- notifies the assigned agent + all admins the first time a ticket crosses into "approaching" or
  "breached" (deduplicated by checking for an existing notification of that type — no separate
  "already notified" column needed)
- auto-escalates any ticket that breaches its SLA, logging a system-generated `TicketActivity`
  (`UserId = null`, rendered as "System" in the timeline)

## Running the project

### Prerequisites
- Docker Desktop (for the one-command path)
- .NET 10 SDK + Node.js 24+ (for local dev without Docker)

### With Docker (recommended)

```bash
cp .env.example .env   # fill in a real JWT_SECRET — see below
docker compose up --build
```

- Frontend: http://localhost:4200
- Backend API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger

The database schema and seed data are applied automatically on first boot (`Database.MigrateAsync()`
+ an idempotent seeder that only runs against an empty `Users` table).

### Without Docker (local dev)

Backend:
```bash
cd backend/HelpDesk.Api
dotnet user-secrets set "Jwt:Secret" "<a-long-random-string-at-least-32-chars>"
dotnet run
```
Requires a local PostgreSQL instance matching `appsettings.Development.json`'s connection string
(or point it at the Dockerized `postgres` service). The JWT signing key is deliberately kept out of
`appsettings.json` — set it via `dotnet user-secrets` locally, or `JWT_SECRET` in `.env` for Docker.

Frontend:
```bash
cd frontend/helpdesk-ui
npm install
npm start
```

## Test credentials

Seeded automatically on first run (password for all: `Password123!`):

| Email | Role |
|---|---|
| admin@example.com | Admin |
| agent1@example.com | Agent |
| agent2@example.com | Agent |
| employee@example.com | Employee |

## Testing

Backend (xUnit, `HelpDesk.Api.Tests`) — pure unit tests for the ticket state machine
(`TicketLifecycle`) and SLA calculation (`SlaCalculator`), plus `TicketService` tests against an EF
Core InMemory database covering ticket creation, SLA due-date calculation, view/edit authorization
(an employee can't see another employee's ticket), illegal status transitions, agent-only actions
(assign/escalate), and priority-change auto-escalation:

```bash
cd backend/HelpDesk.Api.Tests
dotnet test
```

Frontend (Vitest, via the Angular CLI) — guards, interceptors (including the refresh-and-retry flow
and the redirect-to-login on refresh failure), the `Auth` service, reactive form validation, and a
render smoke test for every feature component:

```bash
cd frontend/helpdesk-ui
npm test
```

## Screenshots

| Dashboard | Ticket list |
|---|---|
| ![Dashboard](docs/screenshots/dashboard.png) | ![Ticket list](docs/screenshots/ticket-list.png) |

| Ticket detail (SLA breached) | Reports |
|---|---|
| ![Ticket detail](docs/screenshots/ticket-detail.png) | ![Reports](docs/screenshots/reports.png) |

## Future improvements

- Wire up the `Attachments` entity/DTOs to a real file upload/download endpoint (modeled but not
  implemented)
- Full admin CRUD for users (currently a read-only lookup for the assignee picker) and a richer
  category/department management UI
- A dedicated global search across tickets/comments rather than the ticket list's search field
- Hand-authored migrations were a deliberate workaround for a local tooling block, not a permanent
  choice — on a machine without that restriction, generated migrations (with a `ModelSnapshot`)
  would be preferable going forward

## Project structure

```
helper/
├── backend/
│   ├── HelpDesk.Api/        ASP.NET Core Web API
│   └── HelpDesk.Api.Tests/  xUnit test suite
├── frontend/helpdesk-ui/    Angular application
├── docs/screenshots/        README screenshots
└── docker-compose.yml
```
