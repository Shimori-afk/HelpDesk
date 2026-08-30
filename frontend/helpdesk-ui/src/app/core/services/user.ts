import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { User as UserModel, UserRole } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class User {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/users`;

  getAll(role?: UserRole): Observable<UserModel[]> {
    let params = new HttpParams();
    if (role) params = params.set('role', role);
    return this.http.get<UserModel[]>(this.baseUrl, { params });
  }
}
