import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { Ticket as TicketApi } from '../../../core/services/ticket';
import { Auth } from '../../../core/services/auth';
import { TicketDetail as TicketDetailModel } from '../../../core/models/ticket.model';

@Component({
  selector: 'app-ticket-detail',
  imports: [
    DatePipe,
    NgClass,
    FormsModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatDividerModule
  ],
  templateUrl: './ticket-detail.html',
  styleUrl: './ticket-detail.scss'
})
export class TicketDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly ticketApi = inject(TicketApi);
  protected readonly auth = inject(Auth);

  protected readonly ticket = signal<TicketDetailModel | null>(null);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);
  protected readonly newComment = signal('');
  protected readonly postingComment = signal(false);

  private get ticketId(): number {
    return Number(this.route.snapshot.paramMap.get('id'));
  }

  ngOnInit(): void {
    this.load();
  }

  protected canEdit(): boolean {
    const t = this.ticket();
    const user = this.auth.currentUser();
    if (!t || !user) return false;
    if (user.role === 'Admin') return true;
    return user.id === t.requesterId && (t.status === 'Open' || t.status === 'WaitingForUser');
  }

  protected statusClass(): string {
    return `status-${this.ticket()?.status.toLowerCase()}`;
  }

  protected priorityClass(): string {
    return `priority-${this.ticket()?.priority.toLowerCase()}`;
  }

  protected submitComment(): void {
    const message = this.newComment().trim();
    if (!message) return;

    this.postingComment.set(true);
    this.ticketApi.addComment(this.ticketId, message).subscribe({
      next: () => {
        this.newComment.set('');
        this.postingComment.set(false);
        this.load();
      },
      error: () => this.postingComment.set(false)
    });
  }

  private load(): void {
    this.loading.set(true);
    this.ticketApi.getTicket(this.ticketId).subscribe({
      next: (ticket) => {
        this.ticket.set(ticket);
        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      }
    });
  }
}
