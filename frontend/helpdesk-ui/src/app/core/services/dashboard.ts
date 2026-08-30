import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DashboardSummary } from '../models/dashboard.model';

@Injectable({ providedIn: 'root' })
export class Dashboard {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/dashboard`;

  getSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${this.baseUrl}/summary`);
  }
}
