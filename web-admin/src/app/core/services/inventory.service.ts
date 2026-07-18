import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { InventoryItem, AdjustInventoryRequest } from '../models/models';

@Injectable({ providedIn: 'root' })
export class InventoryService {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/inventory`;

  getAll(locationId?: string): Observable<InventoryItem[]> {
    return this.http.get<InventoryItem[]>(this.base, { params: locationId ? { locationId } : {} });
  }

  adjust(id: string, request: AdjustInventoryRequest): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}`, request);
  }
}
