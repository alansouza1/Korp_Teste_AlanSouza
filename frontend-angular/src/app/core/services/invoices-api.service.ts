import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, Subject } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  AddInvoiceItemsRequest,
  Invoice,
  PrintInvoiceResponse
} from '../models/invoice.models';

@Injectable({ providedIn: 'root' })
export class InvoicesApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.faturamentoApiUrl}/api/invoices`;
  private readonly refreshSubject = new Subject<void>();

  readonly refresh$ = this.refreshSubject.asObservable();

  requestRefresh(): void {
    this.refreshSubject.next();
  }

  getInvoices(): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(this.baseUrl);
  }

  createInvoice(): Observable<Invoice> {
    return this.http.post<Invoice>(this.baseUrl, {});
  }

  getInvoiceById(invoiceId: string): Observable<Invoice> {
    return this.http.get<Invoice>(`${this.baseUrl}/${invoiceId}`);
  }

  addItems(invoiceId: string, payload: AddInvoiceItemsRequest): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.baseUrl}/${invoiceId}/items`, payload);
  }

  printInvoice(invoiceId: string): Observable<PrintInvoiceResponse> {
    return this.http.post<PrintInvoiceResponse>(`${this.baseUrl}/${invoiceId}/print`, {});
  }
}
