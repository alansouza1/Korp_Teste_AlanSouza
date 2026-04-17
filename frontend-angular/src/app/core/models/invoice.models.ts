export type InvoiceStatus = 'OPEN' | 'CLOSED';

export interface InvoiceItem {
  id: string;
  invoiceId: string;
  productCode: string;
  productDescription: string;
  quantity: number;
}

export interface Invoice {
  id: string;
  sequentialNumber: number;
  status: InvoiceStatus;
  createdAt: string;
  updatedAt: string;
  closedAt: string | null;
  printAttempts: number;
  lastPrintError: string | null;
  items: InvoiceItem[];
}

export interface AddInvoiceItemsRequest {
  items: AddInvoiceItem[];
}

export interface AddInvoiceItem {
  productCode: string;
  productDescription: string;
  quantity: number;
}

export interface PrintInvoiceResponse {
  success: boolean;
  message: string;
  invoice: Invoice;
}
