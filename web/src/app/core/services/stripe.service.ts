import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CreateIntentResponse {
  clientSecret: string;
  paymentIntentId: string;
}

/// <summary>
/// Calls the backend to create a Stripe PaymentIntent (see StripeIntentController).
/// The returned clientSecret is what you pass to Stripe.js/Stripe Elements on the
/// client to collect card details and confirm the payment -- see README "Next steps"
/// for wiring up the actual Stripe Elements card form.
/// </summary>
@Injectable({ providedIn: 'root' })
export class StripeService {
  private http = inject(HttpClient);

  createIntent(amount: number, currency = 'usd'): Observable<CreateIntentResponse> {
    return this.http.post<CreateIntentResponse>(`${environment.apiBaseUrl}/payments/stripe/create-intent`, { amount, currency });
  }
}
