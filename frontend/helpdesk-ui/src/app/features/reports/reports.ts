import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData } from 'chart.js';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Report as ReportApi } from '../../core/services/report';
import { ReportsSummary } from '../../core/models/report.model';

const STATUS_COLORS: Record<string, string> = {
  Open: '#3b82f6',
  InProgress: '#d97706',
  WaitingForUser: '#8b5cf6',
  Escalated: '#e11d48',
  Resolved: '#16a34a',
  Closed: '#6b7280',
  Reopened: '#ea580c'
};

const PRIORITY_COLORS: Record<string, string> = {
  Low: '#16a34a',
  Medium: '#d97706',
  High: '#ea580c',
  Critical: '#e11d48'
};

const PALETTE = ['#8b7cf6', '#16a34a', '#d97706', '#3b82f6', '#e11d48', '#0891b2', '#8b5cf6', '#6b7280'];

@Component({
  selector: 'app-reports',
  imports: [BaseChartDirective, MatCardModule, MatProgressSpinnerModule],
  templateUrl: './reports.html',
  styleUrl: './reports.scss'
})
export class Reports implements OnInit {
  private readonly reportApi = inject(ReportApi);

  protected readonly summary = signal<ReportsSummary | null>(null);
  protected readonly loading = signal(true);

  protected readonly ticketsPerDayData = computed<ChartData<'line'>>(() => {
    const items = this.summary()?.ticketsPerDay ?? [];
    return {
      labels: items.map((d) => this.formatShortDate(d.date)),
      datasets: [
        {
          label: 'Tickets created',
          data: items.map((d) => d.count),
          borderColor: '#8b7cf6',
          backgroundColor: 'rgba(139, 124, 246, 0.15)',
          fill: true,
          tension: 0.3
        }
      ]
    };
  });

  protected readonly statusChartData = computed<ChartData<'doughnut'>>(() => {
    const items = this.summary()?.byStatus ?? [];
    return {
      labels: items.map((i) => i.name),
      datasets: [{ data: items.map((i) => i.count), backgroundColor: items.map((i) => STATUS_COLORS[i.name] ?? '#90a4ae') }]
    };
  });

  protected readonly priorityChartData = computed<ChartData<'doughnut'>>(() => {
    const items = this.summary()?.byPriority ?? [];
    return {
      labels: items.map((i) => i.name),
      datasets: [{ data: items.map((i) => i.count), backgroundColor: items.map((i) => PRIORITY_COLORS[i.name] ?? '#90a4ae') }]
    };
  });

  protected readonly categoryChartData = computed<ChartData<'bar'>>(() => {
    const items = this.summary()?.byCategory ?? [];
    return {
      labels: items.map((i) => i.name),
      datasets: [
        {
          label: 'Tickets',
          data: items.map((i) => i.count),
          backgroundColor: items.map((_, idx) => PALETTE[idx % PALETTE.length])
        }
      ]
    };
  });

  protected readonly agentChartData = computed<ChartData<'bar'>>(() => {
    const items = this.summary()?.byAgent ?? [];
    return {
      labels: items.map((i) => i.name),
      datasets: [
        {
          label: 'Assigned tickets',
          data: items.map((i) => i.count),
          backgroundColor: items.map((_, idx) => PALETTE[idx % PALETTE.length])
        }
      ]
    };
  });

  protected readonly barOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    indexAxis: 'y',
    plugins: { legend: { display: false } },
    scales: { x: { ticks: { precision: 0 } } }
  };

  protected readonly lineOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false } },
    scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
  };

  protected readonly doughnutOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { position: 'bottom' } }
  };

  ngOnInit(): void {
    this.reportApi.getSummary().subscribe({
      next: (summary) => {
        this.summary.set(summary);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  protected formatResolutionTime(hours: number | null): string {
    if (hours === null) {
      return 'N/A';
    }
    if (hours < 1) {
      return `${Math.round(hours * 60)}m`;
    }
    const wholeHours = Math.floor(hours);
    const days = Math.floor(wholeHours / 24);
    if (days >= 1) {
      return `${days}d ${wholeHours % 24}h`;
    }
    return `${wholeHours}h ${Math.round((hours - wholeHours) * 60)}m`;
  }

  private formatShortDate(iso: string): string {
    const date = new Date(`${iso}T00:00:00`);
    return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
  }
}
