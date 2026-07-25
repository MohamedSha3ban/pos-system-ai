import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, SessionInfo } from '../models/models';

interface StoredSession {
  fullName: string;
  tenantId: string;
  isPlatformAdmin: boolean;
  roleNames: string[];
  permissions: string[];
}

/// <summary>
/// Holds a short-lived JWT access token (sent on every request) and a longer-lived opaque
/// refresh token (only ever sent to /api/auth/refresh or /api/auth/logout). Both live in
/// localStorage, consistent with the rest of this app -- an httpOnly cookie would be more
/// XSS-resistant but adds CSRF handling and cross-origin cookie complexity that's out of
/// scope for this starter; noted here as a real trade-off, not an oversight.
/// </summary>
@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private accessTokenKey = 'pos_admin_access_token';
  private refreshTokenKey = 'pos_admin_refresh_token';
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

  /// <summary>Exchanges the stored refresh token for a new pair. Used by the auth
  /// interceptor on a 401, and could be called proactively on a timer -- not done here to
  /// keep the starter simple; reactive (401-triggered) refresh is sufficient.</summary>
  refresh(): Observable<AuthResponse | null> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) return of(null);

    return this.http.post<AuthResponse>(`${environment.apiBaseUrl}/auth/refresh`, { refreshToken })
      .pipe(
        tap(res => this.persist(res)),
        catchError(() => { this.clear(); return of(null); })
      );
  }

  /// <summary>Revokes the session server-side, then clears local storage regardless of
  /// whether the server call succeeds (a network hiccup shouldn't trap the user logged in
  /// on this device).</summary>
  logout(): Observable<void> {
    const refreshToken = this.getRefreshToken();
    const clearLocal = () => this.clear();

    if (!refreshToken) { clearLocal(); return of(void 0); }

    return this.http.post<void>(`${environment.apiBaseUrl}/auth/logout`, { refreshToken })
      .pipe(tap(clearLocal), catchError(() => { clearLocal(); return of(void 0); }));
  }

  logoutAllDevices(): Observable<void> {
    return this.http.post<void>(`${environment.apiBaseUrl}/auth/logout-all`, {})
      .pipe(tap(() => this.clear()));
  }

  getSessions(): Observable<SessionInfo[]> {
    const refreshToken = this.getRefreshToken();
    const params = refreshToken ? { currentRefreshToken: refreshToken } : {};
    return this.http.get<SessionInfo[]>(`${environment.apiBaseUrl}/auth/sessions`, { params });
  }

  private persist(res: AuthResponse): void {
    localStorage.setItem(this.accessTokenKey, res.accessToken);
    localStorage.setItem(this.refreshTokenKey, res.refreshToken);
    localStorage.setItem(this.sessionKey, JSON.stringify({
      fullName: res.fullName,
      tenantId: res.tenantId,
      isPlatformAdmin: res.isPlatformAdmin,
      roleNames: res.roleNames,
      permissions: res.permissions
    } as StoredSession));
  }

  private clear(): void {
    localStorage.removeItem(this.accessTokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.sessionKey);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.accessTokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  getSession(): StoredSession | null {
    const raw = localStorage.getItem(this.sessionKey);
    return raw ? JSON.parse(raw) : null;
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }

  hasPermission(code: string): boolean {
    return this.getSession()?.permissions.includes(code) ?? false;
  }

  isPlatformAdmin(): boolean {
    return this.getSession()?.isPlatformAdmin ?? false;
  }
}
