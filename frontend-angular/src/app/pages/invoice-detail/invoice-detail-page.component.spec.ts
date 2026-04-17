import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { convertToParamMap, ActivatedRoute, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { Invoice } from '../../core/models/invoice.models';
import { InvoicesApiService } from '../../core/services/invoices-api.service';
import { InvoiceDetailPageComponent } from './invoice-detail-page.component';

describe('InvoiceDetailPageComponent', () => {
  let fixture: ComponentFixture<InvoiceDetailPageComponent>;
  let invoicesApiSpy: jasmine.SpyObj<InvoicesApiService>;

  const closedInvoice: Invoice = {
    id: 'invoice-1',
    sequentialNumber: 1001,
    status: 'CLOSED',
    createdAt: '2026-04-16T10:00:00Z',
    updatedAt: '2026-04-16T10:05:00Z',
    closedAt: '2026-04-16T10:05:00Z',
    printAttempts: 1,
    lastPrintError: null,
    items: [
      {
        id: 'item-1',
        invoiceId: 'invoice-1',
        productCode: 'P-001',
        productDescription: 'Notebook',
        quantity: 2
      }
    ]
  };

  beforeEach(async () => {
    invoicesApiSpy = jasmine.createSpyObj<InvoicesApiService>('InvoicesApiService', [
      'getInvoiceById',
      'addItems',
      'printInvoice',
      'requestRefresh'
    ]);
    invoicesApiSpy.getInvoiceById.and.returnValue(of(closedInvoice));

    await TestBed.configureTestingModule({
      imports: [InvoiceDetailPageComponent, NoopAnimationsModule],
      providers: [
        provideRouter([]),
        { provide: InvoicesApiService, useValue: invoicesApiSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            paramMap: of(convertToParamMap({ id: closedInvoice.id }))
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(InvoiceDetailPageComponent);
  });

  it('should disable or hide actions when invoice status is CLOSED', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const printButton = fixture.nativeElement.querySelector(
      '.action-row button'
    ) as HTMLButtonElement | null;
    const buttons = Array.from(
      fixture.nativeElement.querySelectorAll('button')
    ) as HTMLButtonElement[];
    const addItemButton = buttons.find((button) => button.textContent?.includes('Add item'));

    expect(invoicesApiSpy.getInvoiceById).toHaveBeenCalledWith(closedInvoice.id);
    expect(printButton).withContext('print button should exist').not.toBeNull();
    expect(printButton?.disabled).toBeTrue();
    expect(addItemButton).toBeUndefined();
    expect(fixture.nativeElement.textContent).toContain('CLOSED invoices are read-only.');
  });
});
