import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CustomerAuthResponse } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AccountAuthService {
  private http = inject(HttpClient);
  private tokenKey = 'storefront_customer_token';
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

  private persist(res: CustomerAuthResponse): void {
    localStorage.setItem(this.tokenKey, res.token);
    localStorage.setItem(this.nameKey, res.fullName);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getFullName(): string | null {
    return localStorage.getItem(this.nameKey);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.nameKey);
  }
}
