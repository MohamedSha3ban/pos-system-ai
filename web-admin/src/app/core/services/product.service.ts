import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Product, AdminProduct, UpsertProductRequest } from '../models/models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/products`;

  getCatalog(locationId: string): Observable<Product[]> {
    return this.http.get<Product[]>(this.base, { params: { locationId } });
  }

  getById(id: string, locationId: string): Observable<AdminProduct> {
    return this.http.get<AdminProduct>(`${this.base}/${id}`, { params: { locationId } });
  }

  create(product: UpsertProductRequest, locationId: string, initialQuantity: number): Observable<AdminProduct> {
    return this.http.post<AdminProduct>(this.base, { product, locationId, initialQuantity });
  }

  update(id: string, product: UpsertProductRequest, locationId: string): Observable<AdminProduct> {
    return this.http.put<AdminProduct>(`${this.base}/${id}`, product, { params: { locationId } });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  adjustStock(id: string, locationId: string, quantity: number): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}/stock`, null, { params: { locationId, quantity } });
  }
}
