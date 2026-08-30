import { Component, OnDestroy, OnInit, effect, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DatePipe } from '@angular/common';
import { Auth } from '../../core/services/auth';
import { Signalr } from '../../core/services/signalr';
import { Notification as NotificationApi } from '../../core/services/notification';
import { AppNotification } from '../../core/models/notification.model';

@Component({
  selector: 'app-shell',
  imports: [
    DatePipe,
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatBadgeModule
  ],
  templateUrl: './shell.html',
  styleUrl: './shell.scss'
})
export class Shell implements OnInit, OnDestroy {
  protected readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly signalr = inject(Signalr);
  private readonly notificationApi = inject(NotificationApi);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly navItems = [
    { label: 'Dashboard', icon: 'dashboard', path: '/dashboard' },
    { label: 'Tickets', icon: 'confirmation_number', path: '/tickets' },
    { label: 'New Ticket', icon: 'add_circle', path: '/tickets/new' }
  ];

  protected readonly notifications = signal<AppNotification[]>([]);
  protected readonly unreadCount = signal(0);

  constructor() {
    effect(() => {
      const incoming = this.signalr.notificationReceived();
      if (incoming) {
        this.notifications.update((list) => [incoming, ...list]);
        this.recomputeUnread();
        this.snackBar.open(incoming.message, 'View', { duration: 6000 });
      }
    });

    effect(() => {
      const ticket = this.signalr.ticketCreated();
      if (ticket && this.auth.hasRole('Agent', 'Admin')) {
        this.snackBar.open(`🔔 New ticket #${ticket.id}: ${ticket.title} (${ticket.priority})`, 'View', {
          duration: 8000
        });
      }
    });
  }

  ngOnInit(): void {
    this.loadNotifications();
    this.signalr.start();
  }

  ngOnDestroy(): void {
    this.signalr.stop();
  }

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }

  protected openNotification(notification: AppNotification): void {
    if (!notification.isRead) {
      this.notificationApi.markAsRead(notification.id).subscribe(() => {
        notification.isRead = true;
        this.recomputeUnread();
      });
    }
    if (notification.ticketId) {
      this.router.navigate(['/tickets', notification.ticketId]);
    }
  }

  protected markAllRead(): void {
    this.notificationApi.markAllAsRead().subscribe(() => {
      this.notifications.update((list) => list.map((n) => ({ ...n, isRead: true })));
      this.unreadCount.set(0);
    });
  }

  private loadNotifications(): void {
    this.notificationApi.getAll().subscribe((notifications) => {
      this.notifications.set(notifications);
      this.recomputeUnread();
    });
  }

  private recomputeUnread(): void {
    this.unreadCount.set(this.notifications().filter((n) => !n.isRead).length);
  }
}
