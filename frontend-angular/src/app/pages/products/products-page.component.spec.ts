import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { Product } from '../../core/models/product.models';
import { ProductsApiService } from '../../core/services/products-api.service';
import { ProductsPageComponent } from './products-page.component';

describe('ProductsPageComponent', () => {
  let fixture: ComponentFixture<ProductsPageComponent>;
  let productsApiSpy: jasmine.SpyObj<ProductsApiService>;

  const products: Product[] = [
    {
      id: 'product-1',
      code: 'P-001',
      description: 'Notebook',
      stockQuantity: 15,
      createdAt: '2026-04-16T10:00:00Z',
      updatedAt: '2026-04-16T10:00:00Z'
    },
    {
      id: 'product-2',
      code: 'P-002',
      description: 'Keyboard',
      stockQuantity: 8,
      createdAt: '2026-04-16T11:00:00Z',
      updatedAt: '2026-04-16T11:00:00Z'
    }
  ];

  beforeEach(async () => {
    productsApiSpy = jasmine.createSpyObj<ProductsApiService>('ProductsApiService', [
      'getProducts',
      'suggestDescription',
      'createProduct',
      'updateDescription',
      'updateStock'
    ]);
    productsApiSpy.getProducts.and.returnValue(of(products));

    await TestBed.configureTestingModule({
      imports: [ProductsPageComponent, NoopAnimationsModule],
      providers: [{ provide: ProductsApiService, useValue: productsApiSpy }]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductsPageComponent);
  });

  it('should render without crashing', () => {
    fixture.detectChanges();

    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should display product list when data is loaded', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const rows = Array.from(
      fixture.nativeElement.querySelectorAll('tr.mat-mdc-row')
    ) as HTMLElement[];

    expect(productsApiSpy.getProducts).toHaveBeenCalled();
    expect(rows.length).toBe(2);
    expect(fixture.nativeElement.textContent).toContain('P-001');
    expect(fixture.nativeElement.textContent).toContain('Notebook');
    expect(fixture.nativeElement.textContent).toContain('P-002');
  });
});
