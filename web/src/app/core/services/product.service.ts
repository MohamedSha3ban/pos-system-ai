import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Product } from '../models/models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private http = inject(HttpClient);

  getCatalog(locationId: string): Observable<Product[]> {
    return this.http.get<Product[]>(`${environment.apiBaseUrl}/products`, { params: { locationId } });
  }
}
