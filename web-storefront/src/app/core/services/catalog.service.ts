import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Product } from '../models/models';

/// <summary>Public product browsing -- no login required (matches StorefrontCatalogController, [AllowAnonymous]).</summary>
@Injectable({ providedIn: 'root' })
export class CatalogService {
  private http = inject(HttpClient);

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(
      `${environment.apiBaseUrl}/storefront/${environment.tenantId}/products`,
      { params: { locationId: environment.locationId } }
    );
  }
}
