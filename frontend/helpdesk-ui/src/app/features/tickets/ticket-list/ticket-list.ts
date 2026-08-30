import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';
import { Ticket as TicketApi } from '../../../core/services/ticket';
import { Auth } from '../../../core/services/auth';
import { SlaState, TicketListItem, TicketPriority, TicketQuery, TicketStatus } from '../../../core/models/ticket.model';

const STATUSES: TicketStatus[] = ['Open', 'InProgress', 'WaitingForUser', 'Escalated', 'Resolved', 'Closed', 'Reopened'];
const PRIORITIES: TicketPriority[] = ['Low', 'Medium', 'High', 'Critical'];

@Component({
  selector: 'app-ticket-list',
  imports: [
    DatePipe,
    NgClass,
    FormsModule,
    RouterLink,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './ticket-list.html',
  styleUrl: './ticket-list.scss'
})
export class TicketList implements OnInit {
  private readonly ticketApi = inject(TicketApi);
  private readonly route = inject(ActivatedRoute);
  protected readonly auth = inject(Auth);
  private readonly searchInput$ = new Subject<string>();

  protected readonly statuses = STATUSES;
  protected readonly priorities = PRIORITIES;
  protected readonly displayedColumns = [
    'id', 'title', 'requester', 'category', 'priority', 'status', 'assignedAgent', 'createdAt', 'sla'
  ];

  protected readonly tickets = signal<TicketListItem[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly loading = signal(false);

  protected search = '';
  protected statusFilter: TicketStatus | '' = '';
  protected priorityFilter: TicketPriority | '' = '';
  protected quickFilter: 'all' | 'mine' | 'unassigned' = 'all';
  protected page = 0;
  protected pageSize = 20;
  protected overdueOnly = false;

  ngOnInit(): void {
    this.applyInitialFiltersFromRoute();

    this.searchInput$.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => {
      this.page = 0;
      this.load();
    });
    this.load();
  }

  private applyInitialFiltersFromRoute(): void {
    const params = this.route.snapshot.queryParamMap;
    const status = params.get('status');
    const priority = params.get('priority');
    const assignedAgentId = params.get('assignedAgentId');

    if (status && (this.statuses as string[]).includes(status)) {
      this.statusFilter = status as TicketStatus;
    }
    if (priority && (this.priorities as string[]).includes(priority)) {
      this.priorityFilter = priority as TicketPriority;
    }
    if (params.get('unassigned') === 'true') {
      this.quickFilter = 'unassigned';
    } else if (assignedAgentId && Number(assignedAgentId) === this.auth.currentUser()?.id) {
      this.quickFilter = 'mine';
    }
    this.overdueOnly = params.get('overdue') === 'true';
  }

  onSearchChange(value: string): void {
    this.search = value;
    this.searchInput$.next(value);
  }

  onFilterChange(): void {
    this.page = 0;
    this.load();
  }

  clearOverdueFilter(): void {
    this.overdueOnly = false;
    this.page = 0;
    this.load();
  }

  setQuickFilter(filter: 'all' | 'mine' | 'unassigned'): void {
    this.quickFilter = filter;
    this.page = 0;
    this.load();
  }

  onPageChange(event: PageEvent): void {
    this.page = event.pageIndex;
    this.pageSize = event.pageSize;
    this.load();
  }

  protected statusClass(status: TicketStatus): string {
    return `status-${status.toLowerCase()}`;
  }

  protected priorityClass(priority: TicketPriority): string {
    return `priority-${priority.toLowerCase()}`;
  }

  protected slaClass(state: SlaState): string {
    return `sla-${state.toLowerCase()}`;
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

  private load(): void {
    this.loading.set(true);
    const query: TicketQuery = {
      search: this.search || undefined,
      status: this.statusFilter || undefined,
      priority: this.priorityFilter || undefined,
      assignedAgentId: this.quickFilter === 'mine' ? this.auth.currentUser()?.id : undefined,
      unassigned: this.quickFilter === 'unassigned' ? true : undefined,
      overdue: this.overdueOnly || undefined,
      page: this.page + 1,
      pageSize: this.pageSize
    };

    this.ticketApi.getTickets(query).subscribe({
      next: (result) => {
        this.tickets.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
