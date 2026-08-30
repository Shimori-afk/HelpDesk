import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell/shell').then((m) => m.Shell),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard').then((m) => m.Dashboard)
      },
      {
        path: 'tickets',
        loadComponent: () => import('./features/tickets/ticket-list/ticket-list').then((m) => m.TicketList)
      },
      {
        path: 'tickets/new',
        loadComponent: () => import('./features/tickets/ticket-create/ticket-create').then((m) => m.TicketCreate)
      },
      {
        path: 'tickets/:id/edit',
        loadComponent: () => import('./features/tickets/ticket-create/ticket-create').then((m) => m.TicketCreate)
      },
      {
        path: 'tickets/:id',
        loadComponent: () => import('./features/tickets/ticket-detail/ticket-detail').then((m) => m.TicketDetail)
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
