import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { Auth } from './auth';
import { AppNotification } from '../models/notification.model';
import { TicketDetail } from '../models/ticket.model';

@Injectable({ providedIn: 'root' })
export class Signalr {
  private readonly auth = inject(Auth);
  private connection: signalR.HubConnection | null = null;

  readonly connectionState = signal<signalR.HubConnectionState>(signalR.HubConnectionState.Disconnected);
  readonly notificationReceived = signal<AppNotification | null>(null);
  readonly ticketCreated = signal<TicketDetail | null>(null);

  async start(): Promise<void> {
    if (this.connection) return;

    const token = this.auth.getAccessToken();
    if (!token) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.signalRUrl}/notifications`, {
        accessTokenFactory: () => this.auth.getAccessToken() ?? ''
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.connection.on('notificationReceived', (notification: AppNotification) => {
      this.notificationReceived.set(notification);
    });

    this.connection.on('ticketCreated', (ticket: TicketDetail) => {
      this.ticketCreated.set(ticket);
    });

    this.connection.onreconnecting(() => this.connectionState.set(signalR.HubConnectionState.Reconnecting));
    this.connection.onreconnected(() => this.connectionState.set(signalR.HubConnectionState.Connected));
    this.connection.onclose(() => this.connectionState.set(signalR.HubConnectionState.Disconnected));

    try {
      await this.connection.start();
      this.connectionState.set(signalR.HubConnectionState.Connected);
    } catch {
      // The UI stays fully usable without a live connection; the app just
      // won't get push updates until the next reconnect attempt or reload.
      this.connectionState.set(signalR.HubConnectionState.Disconnected);
    }
  }

  async stop(): Promise<void> {
    if (!this.connection) return;
    await this.connection.stop();
    this.connection = null;
    this.connectionState.set(signalR.HubConnectionState.Disconnected);
  }
}
