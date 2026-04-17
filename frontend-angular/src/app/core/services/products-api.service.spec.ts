import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { ProductsApiService } from './products-api.service';

describe('ProductsApiService', () => {
  let service: ProductsApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
    });

    service = TestBed.inject(ProductsApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should call the stock update endpoint using PATCH', () => {
    const payload = { stockQuantity: 12 };

    service.updateStock('product-123', payload).subscribe();

    const request = httpMock.expectOne(
      'http://localhost:5001/api/products/product-123/stock'
    );

    expect(request.request.method).toBe('PATCH');
    expect(request.request.body).toEqual(payload);

    request.flush({
      id: 'product-123',
      code: 'P-001',
      description: 'Notebook',
      stockQuantity: 12,
      createdAt: '2026-04-16T10:00:00Z',
      updatedAt: '2026-04-16T10:10:00Z'
    });
  });
});
