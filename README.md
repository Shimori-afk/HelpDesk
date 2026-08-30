# HelpDesk

A full-stack internal IT support ticketing system built with Angular, ASP.NET Core, and PostgreSQL.

> Work in progress — being built incrementally, phase by phase. This README will grow as features land.

## Technology Stack

- **Frontend:** Angular 22 (standalone, zoneless), Angular Material, SignalR client
- **Backend:** ASP.NET Core 10 Web API, Entity Framework Core, JWT auth, SignalR
- **Database:** PostgreSQL 17
- **Infrastructure:** Docker Compose

## Project Structure

```
helper/
├── backend/HelpDesk.Api/     ASP.NET Core Web API
├── frontend/helpdesk-ui/     Angular application
└── docker-compose.yml
```

## Running Locally

### Prerequisites
- .NET 10 SDK
- Node.js 24+
- Docker Desktop

### With Docker

```bash
cp .env.example .env   # adjust values as needed
docker compose up --build
```

- Frontend: http://localhost:4200
- Backend API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger

### Without Docker (dev mode)

Backend:
```bash
cd backend/HelpDesk.Api
dotnet user-secrets set "Jwt:Secret" "<a-long-random-string-at-least-32-chars>"
dotnet run
```

The JWT signing key is deliberately kept out of `appsettings.json` — set it via `dotnet user-secrets` locally (as above) or the `JWT_SECRET` value in `.env` when running through Docker.

Frontend:
```bash
cd frontend/helpdesk-ui
npm install
npm start
```

## Test Credentials

Seeded automatically on first run (password for all: `Password123!`):

| Email | Role |
|---|---|
| admin@example.com | Admin |
| agent1@example.com | Agent |
| agent2@example.com | Agent |
| employee@example.com | Employee |

## Status

- [x] Phase 1 — Project setup (Angular, ASP.NET Core, PostgreSQL, Docker, EF Core, Swagger)
- [x] Phase 2 — Database schema & seed data
- [x] Phase 3 — Authentication & authorization
- [x] Phase 4 — Ticket system
- [x] Phase 5 — Agent functionality
- [x] Phase 6 — SignalR real-time updates
- [ ] Phase 7 — SLA monitoring & background service
- [ ] Phase 8 — Dashboard
- [ ] Phase 9 — Reports
- [ ] Phase 10 — Testing, Docker hardening, final README
