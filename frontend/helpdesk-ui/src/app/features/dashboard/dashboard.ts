import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Auth } from '../../core/services/auth';
import { Dashboard as DashboardApi } from '../../core/services/dashboard';
import { DashboardSummary } from '../../core/models/dashboard.model';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  protected readonly auth = inject(Auth);
  private readonly dashboardApi = inject(DashboardApi);

  protected readonly summary = signal<DashboardSummary | null>(null);
  protected readonly loading = signal(true);
  protected readonly isAgentOrAdmin = computed(() => this.auth.hasRole('Agent', 'Admin'));

  ngOnInit(): void {
    this.dashboardApi.getSummary().subscribe({
      next: (summary) => {
        this.summary.set(summary);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
