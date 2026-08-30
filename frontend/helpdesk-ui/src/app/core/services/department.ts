import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Department as DepartmentModel } from '../models/lookup.model';

@Injectable({ providedIn: 'root' })
export class Department {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/departments`;

  getAll(): Observable<DepartmentModel[]> {
    return this.http.get<DepartmentModel[]>(this.baseUrl);
  }

  create(name: string): Observable<DepartmentModel> {
    return this.http.post<DepartmentModel>(this.baseUrl, { name });
  }

  update(id: number, name: string): Observable<DepartmentModel> {
    return this.http.put<DepartmentModel>(`${this.baseUrl}/${id}`, { name });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
