import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ReportsSummary } from '../models/report.model';

@Injectable({ providedIn: 'root' })
export class Report {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/reports`;

  getSummary(): Observable<ReportsSummary> {
    return this.http.get<ReportsSummary>(`${this.baseUrl}/summary`);
  }
}
