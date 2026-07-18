import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CreateIntentResponse {
  clientSecret: string;
  paymentIntentId: string;
}

@Injectable({ providedIn: 'root' })
export class StripeService {
  private http = inject(HttpClient);

  createIntent(amount: number, currency = 'usd'): Observable<CreateIntentResponse> {
    return this.http.post<CreateIntentResponse>(`${environment.apiBaseUrl}/payments/stripe/create-intent`, { amount, currency });
  }
}
