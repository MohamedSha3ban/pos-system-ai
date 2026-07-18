import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private tokenKey = 'pos_admin_token';
  private sessionKey = 'pos_admin_session';

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiBaseUrl}/auth/login`, { email, password })
      .pipe(tap(res => this.persist(res)));
  }

  registerTenant(payload: {
    businessName: string; businessType: string;
    ownerFullName: string; ownerEmail: string; ownerPassword: string;
  }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiBaseUrl}/auth/register-tenant`, payload)
      .pipe(tap(res => this.persist(res)));
  }

  private persist(res: AuthResponse): void {
    localStorage.setItem(this.tokenKey, res.token);
    localStorage.setItem(this.sessionKey, JSON.stringify({
      fullName: res.fullName,
      tenantId: res.tenantId,
      isPlatformAdmin: res.isPlatformAdmin,
      roleNames: res.roleNames,
      permissions: res.permissions
    }));
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getSession(): { fullName: string; tenantId: string; isPlatformAdmin: boolean; roleNames: string[]; permissions: string[] } | null {
    const raw = localStorage.getItem(this.sessionKey);
    return raw ? JSON.parse(raw) : null;
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  hasPermission(code: string): boolean {
    return this.getSession()?.permissions.includes(code) ?? false;
  }

  isPlatformAdmin(): boolean {
    return this.getSession()?.isPlatformAdmin ?? false;
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.sessionKey);
  }
}
