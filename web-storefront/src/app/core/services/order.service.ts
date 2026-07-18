import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CartLine, OrderResponse, Tender } from '../models/models';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private http = inject(HttpClient);

  checkout(cart: CartLine[], tenders: Tender[], tipTotal = 0): Observable<OrderResponse> {
    const body = {
      locationId: environment.locationId,
      customerId: null,
      items: cart.map(c => ({ productId: c.product.id, quantity: c.quantity, lineDiscount: 0 })),
      tenders: tenders.map(t => ({ method: t.method, amount: t.amount, paymentToken: t.paymentToken ?? null })),
      tipTotal
    };
    return this.http.post<OrderResponse>(`${environment.apiBaseUrl}/storefront/checkout`, body);
  }
}
