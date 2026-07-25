import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CustomerAuthResponse } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AccountAuthService {
  private http = inject(HttpClient);
  private accessTokenKey = 'storefront_customer_access_token';
  private refreshTokenKey = 'storefront_customer_refresh_token';
  private nameKey = 'storefront_customer_name';

  private get base(): string {
    return `${environment.apiBaseUrl}/storefront/${environment.tenantId}/auth`;
  }

  register(fullName: string, email: string, password: string, phone?: string): Observable<CustomerAuthResponse> {
    return this.http.post<CustomerAuthResponse>(`${this.base}/register`, { fullName, email, password, phone })
      .pipe(tap(res => this.persist(res)));
  }

  login(email: string, password: string): Observable<CustomerAuthResponse> {
    return this.http.post<CustomerAuthResponse>(`${this.base}/login`, { email, password })
      .pipe(tap(res => this.persist(res)));
  }

  /// <summary>Used by the auth interceptor on a 401 to silently re-auth without booting the shopper to the login screen.</summary>
  refresh(): Observable<CustomerAuthResponse | null> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) return of(null);

    return this.http.post<CustomerAuthResponse>(`${this.base}/refresh`, { refreshToken })
      .pipe(
        tap(res => this.persist(res)),
        catchError(() => { this.clear(); return of(null); })
      );
  }

  logout(): Observable<void> {
    const refreshToken = this.getRefreshToken();
    const clearLocal = () => this.clear();

    if (!refreshToken) { clearLocal(); return of(void 0); }

    return this.http.post<void>(`${this.base}/logout`, { refreshToken })
      .pipe(tap(clearLocal), catchError(() => { clearLocal(); return of(void 0); }));
  }

  private persist(res: CustomerAuthResponse): void {
    localStorage.setItem(this.accessTokenKey, res.accessToken);
    localStorage.setItem(this.refreshTokenKey, res.refreshToken);
    localStorage.setItem(this.nameKey, res.fullName);
  }

  private clear(): void {
    localStorage.removeItem(this.accessTokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.nameKey);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.accessTokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  getFullName(): string | null {
    return localStorage.getItem(this.nameKey);
  }

  isLoggedIn(): boolean {
    return !!this.getAccessToken();
  }
}
