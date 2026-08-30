import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Category as CategoryModel } from '../models/lookup.model';

@Injectable({ providedIn: 'root' })
export class Category {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/categories`;

  getAll(): Observable<CategoryModel[]> {
    return this.http.get<CategoryModel[]>(this.baseUrl);
  }

  create(name: string): Observable<CategoryModel> {
    return this.http.post<CategoryModel>(this.baseUrl, { name });
  }

  update(id: number, name: string): Observable<CategoryModel> {
    return this.http.put<CategoryModel>(`${this.baseUrl}/${id}`, { name });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
