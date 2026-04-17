import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  CreateProductRequest,
  Product,
  UpdateProductDescriptionRequest,
  UpdateProductStockRequest
} from '../models/product.models';
import {
  SuggestProductDescriptionRequest,
  SuggestProductDescriptionResponse
} from '../models/product-description-suggestion.models';

@Injectable({ providedIn: 'root' })
export class ProductsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.estoqueApiUrl}/api/products`;

  getProducts(filters?: { code?: string; description?: string }): Observable<Product[]> {
    let params = new HttpParams();

    if (filters?.code?.trim()) {
      params = params.set('code', filters.code.trim());
    }

    if (filters?.description?.trim()) {
      params = params.set('description', filters.description.trim());
    }

    return this.http.get<Product[]>(this.baseUrl, { params });
  }

  createProduct(payload: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.baseUrl, payload);
  }

  suggestDescription(payload: SuggestProductDescriptionRequest): Observable<SuggestProductDescriptionResponse> {
    return this.http.post<SuggestProductDescriptionResponse>(`${this.baseUrl}/description-suggestions`, payload);
  }

  updateDescription(productId: string, payload: UpdateProductDescriptionRequest): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}/${productId}`, payload);
  }

  updateStock(productId: string, payload: UpdateProductStockRequest): Observable<Product> {
    return this.http.patch<Product>(`${this.baseUrl}/${productId}/stock`, payload);
  }
}
