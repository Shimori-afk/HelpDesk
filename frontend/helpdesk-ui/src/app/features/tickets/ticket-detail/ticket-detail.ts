import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Ticket as TicketApi } from '../../../core/services/ticket';
import { User as UserApi } from '../../../core/services/user';
import { Auth } from '../../../core/services/auth';
import { SlaState, TicketDetail as TicketDetailModel, TicketPriority, TicketStatus } from '../../../core/models/ticket.model';
import { User } from '../../../core/models/user.model';
import { ApiError } from '../../../core/models/api-error.model';

const NEXT_STATUSES: Record<TicketStatus, TicketStatus[]> = {
  Open: ['InProgress', 'Escalated', 'Closed'],
  InProgress: ['WaitingForUser', 'Resolved', 'Escalated'],
  WaitingForUser: ['InProgress', 'Resolved', 'Escalated'],
  Escalated: ['InProgress', 'Resolved'],
  Resolved: ['Closed', 'Reopened'],
  Reopened: ['InProgress'],
  Closed: []
};

const PRIORITIES: TicketPriority[] = ['Low', 'Medium', 'High', 'Critical'];

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
    MatSelectModule,
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
  private readonly userApi = inject(UserApi);
  private readonly snackBar = inject(MatSnackBar);
  protected readonly auth = inject(Auth);

  protected readonly priorities = PRIORITIES;
  protected readonly ticket = signal<TicketDetailModel | null>(null);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);
  protected readonly newComment = signal('');
  protected readonly postingComment = signal(false);
  protected readonly actionLoading = signal(false);
  protected readonly agents = signal<User[]>([]);

  protected readonly nextStatuses = computed(() => NEXT_STATUSES[this.ticket()?.status ?? 'Open']);
  protected readonly isAgentOrAdmin = computed(() => this.auth.hasRole('Agent', 'Admin'));

  private get ticketId(): number {
    return Number(this.route.snapshot.paramMap.get('id'));
  }

  ngOnInit(): void {
    this.load();
    if (this.isAgentOrAdmin()) {
      this.userApi.getAll('Agent').subscribe((agents) => this.agents.set(agents));
    }
  }

  protected canEdit(): boolean {
    const t = this.ticket();
    const user = this.auth.currentUser();
    if (!t || !user) return false;
    if (user.role === 'Admin') return true;
    return user.id === t.requesterId && (t.status === 'Open' || t.status === 'WaitingForUser');
  }

  protected canReopen(): boolean {
    const t = this.ticket();
    const user = this.auth.currentUser();
    return !!t && !!user && t.status === 'Resolved' && user.id === t.requesterId;
  }

  protected statusClass(): string {
    return `status-${this.ticket()?.status.toLowerCase()}`;
  }

  protected priorityClass(): string {
    return `priority-${this.ticket()?.priority.toLowerCase()}`;
  }

  protected slaClass(): string {
    return `sla-${(this.ticket()?.slaState ?? 'WithinSla').toLowerCase()}`;
  }

  protected slaLabel(state: SlaState): string {
    switch (state) {
      case 'Breached':
        return 'SLA breached';
      case 'ApproachingBreach':
        return 'Approaching SLA deadline';
      default:
        return 'Within SLA';
    }
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

  protected changeStatus(status: TicketStatus): void {
    this.runAction(this.ticketApi.changeStatus(this.ticketId, status));
  }

  protected reopen(): void {
    this.runAction(this.ticketApi.changeStatus(this.ticketId, 'Reopened'));
  }

  protected assign(agentId: number | null): void {
    this.runAction(this.ticketApi.assign(this.ticketId, agentId));
  }

  protected changePriority(priority: TicketPriority): void {
    this.runAction(this.ticketApi.changePriority(this.ticketId, priority));
  }

  protected escalate(): void {
    this.runAction(this.ticketApi.escalate(this.ticketId));
  }

  private runAction(request$: ReturnType<typeof this.ticketApi.changeStatus>): void {
    this.actionLoading.set(true);
    request$.subscribe({
      next: (ticket) => {
        this.ticket.set(ticket);
        this.actionLoading.set(false);
      },
      error: (error: unknown) => {
        this.actionLoading.set(false);
        const apiError = error instanceof HttpErrorResponse ? (error.error as ApiError) : undefined;
        this.snackBar.open(apiError?.title ?? 'Action failed.', 'Dismiss', { duration: 4000 });
      }
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
