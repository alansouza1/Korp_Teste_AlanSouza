import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { convertToParamMap, ActivatedRoute, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { Invoice } from '../../core/models/invoice.models';
import { InvoicesApiService } from '../../core/services/invoices-api.service';
import { Product } from '../../core/models/product.models';
import { ProductsApiService } from '../../core/services/products-api.service';
import { InvoiceDetailPageComponent } from './invoice-detail-page.component';

describe('InvoiceDetailPageComponent', () => {
  let fixture: ComponentFixture<InvoiceDetailPageComponent>;
  let invoicesApiSpy: jasmine.SpyObj<InvoicesApiService>;
  let productsApiSpy: jasmine.SpyObj<ProductsApiService>;

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

  const products: Product[] = [
    {
      id: 'product-1',
      code: 'P-001',
      description: 'Notebook corporativo',
      stockQuantity: 10,
      createdAt: '2026-04-16T10:00:00Z',
      updatedAt: '2026-04-16T10:00:00Z'
    },
    {
      id: 'product-2',
      code: 'M-001',
      description: 'Mouse sem fio',
      stockQuantity: 20,
      createdAt: '2026-04-16T10:00:00Z',
      updatedAt: '2026-04-16T10:00:00Z'
    }
  ];

  beforeEach(async () => {
    invoicesApiSpy = jasmine.createSpyObj<InvoicesApiService>('InvoicesApiService', [
      'getInvoiceById',
      'addItems',
      'printInvoice',
      'requestRefresh'
    ]);
    productsApiSpy = jasmine.createSpyObj<ProductsApiService>('ProductsApiService', ['getProducts']);
    invoicesApiSpy.getInvoiceById.and.returnValue(of(closedInvoice));
    productsApiSpy.getProducts.and.returnValue(of(products));

    await TestBed.configureTestingModule({
      imports: [InvoiceDetailPageComponent, NoopAnimationsModule],
      providers: [
        provideRouter([]),
        { provide: InvoicesApiService, useValue: invoicesApiSpy },
        { provide: ProductsApiService, useValue: productsApiSpy },
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
    expect(fixture.nativeElement.textContent).toContain('Notas fiscais fechadas são somente leitura.');
  });

  it('should load the product catalog for item selection', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    expect(productsApiSpy.getProducts).toHaveBeenCalled();
  });

  it('should filter products by code or description', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.productSearchControl.setValue('mouse');

    const filtered = await Promise.resolve(component['filterProducts'](products, 'mouse'));

    expect(filtered.length).toBe(1);
    expect(filtered[0].code).toBe('M-001');
  });

  it('should update the item form after selecting a product', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    component.selectProduct(products[0]);

    expect(component.addItemForm.controls.productCode.getRawValue()).toBe('P-001');
    expect(component.addItemForm.controls.productDescription.getRawValue()).toBe('Notebook corporativo');
    expect(component.productSearchControl.getRawValue()).toContain('P-001');
  });
});
