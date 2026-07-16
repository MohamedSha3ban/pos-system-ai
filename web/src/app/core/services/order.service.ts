import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CartLine, OrderResponse } from '../models/models';

export interface Tender { method: string; amount: number; }

@Injectable({ providedIn: 'root' })
export class OrderService {
  private http = inject(HttpClient);

  checkout(locationId: string, cart: CartLine[], tenders: Tender[], tipTotal = 0): Observable<OrderResponse> {
    const body = {
      locationId,
      customerId: null,
      items: cart.map(c => ({ productId: c.product.id, quantity: c.quantity, lineDiscount: 0 })),
      tenders,
      tipTotal
    };
    return this.http.post<OrderResponse>(`${environment.apiBaseUrl}/orders/checkout`, body);
  }
}
