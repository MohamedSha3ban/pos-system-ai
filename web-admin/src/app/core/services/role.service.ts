import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Role, UpsertRoleRequest } from '../models/models';

@Injectable({ providedIn: 'root' })
export class RoleService {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/roles`;

  getAll(): Observable<Role[]> {
    return this.http.get<Role[]>(this.base);
  }

  getAvailablePermissions(): Observable<string[]> {
    return this.http.get<string[]>(`${environment.apiBaseUrl}/permissions`);
  }

  create(request: UpsertRoleRequest): Observable<Role> {
    return this.http.post<Role>(this.base, request);
  }

  update(id: string, request: UpsertRoleRequest): Observable<Role> {
    return this.http.put<Role>(`${this.base}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
