import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateTicketRequest,
  PagedResult,
  TicketActivity,
  TicketComment,
  TicketDetail,
  TicketListItem,
  TicketPriority,
  TicketQuery,
  TicketStatus,
  UpdateTicketRequest
} from '../models/ticket.model';

@Injectable({ providedIn: 'root' })
export class Ticket {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/tickets`;

  getTickets(query: TicketQuery): Observable<PagedResult<TicketListItem>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<PagedResult<TicketListItem>>(this.baseUrl, { params });
  }

  getTicket(id: number): Observable<TicketDetail> {
    return this.http.get<TicketDetail>(`${this.baseUrl}/${id}`);
  }

  createTicket(request: CreateTicketRequest): Observable<TicketDetail> {
    return this.http.post<TicketDetail>(this.baseUrl, request);
  }

  updateTicket(id: number, request: UpdateTicketRequest): Observable<TicketDetail> {
    return this.http.put<TicketDetail>(`${this.baseUrl}/${id}`, request);
  }

  deleteTicket(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getActivities(id: number): Observable<TicketActivity[]> {
    return this.http.get<TicketActivity[]>(`${this.baseUrl}/${id}/activities`);
  }

  addComment(id: number, message: string): Observable<TicketComment> {
    return this.http.post<TicketComment>(`${this.baseUrl}/${id}/comments`, { message });
  }

  changeStatus(id: number, status: TicketStatus): Observable<TicketDetail> {
    return this.http.put<TicketDetail>(`${this.baseUrl}/${id}/status`, { status });
  }

  assign(id: number, agentId: number | null): Observable<TicketDetail> {
    return this.http.put<TicketDetail>(`${this.baseUrl}/${id}/assign`, { agentId });
  }

  changePriority(id: number, priority: TicketPriority): Observable<TicketDetail> {
    return this.http.put<TicketDetail>(`${this.baseUrl}/${id}/priority`, { priority });
  }

  escalate(id: number): Observable<TicketDetail> {
    return this.http.post<TicketDetail>(`${this.baseUrl}/${id}/escalate`, {});
  }
}
