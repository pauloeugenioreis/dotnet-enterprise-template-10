import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ProductService } from './product.service';
import { CreateProductRequest } from '../../shared/models';

describe('ProductService', () => {
  let service: ProductService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        ProductService
      ]
    });
    service = TestBed.inject(ProductService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get products with filters', () => {
    service.getProducts(1, 10, 'phone', true).subscribe();

    const req = httpMock.expectOne(r => 
      r.url.includes('/api/v1/Product?page=1&pageSize=10') &&
      r.url.includes('searchTerm=phone') &&
      r.url.includes('isActive=true')
    );
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should create a product', () => {
    const mockProduct: CreateProductRequest = { 
      name: 'New Product', 
      category: 'Electronics',
      price: 100,
      stock: 10,
      isActive: true
    };
    service.create(mockProduct).subscribe();

    const req = httpMock.expectOne(r => r.url.includes('/api/v1/Product'));
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockProduct);
    req.flush({});
  });

  it('should update a product', () => {
    const productId = '123';
    const mockProduct: CreateProductRequest = { 
      name: 'Updated Product',
      category: 'Electronics',
      price: 120,
      stock: 5,
      isActive: true
    };
    service.update(productId, mockProduct).subscribe();

    const req = httpMock.expectOne(r => r.url.endsWith(`/api/v1/Product/${productId}`));
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(mockProduct);
    req.flush({});
  });

  it('should delete a product', () => {
    const productId = '123';
    service.delete(productId).subscribe();

    const req = httpMock.expectOne(r => r.url.endsWith(`/api/v1/Product/${productId}`));
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });

  it('should export products to excel', () => {
    service.exportToExcel('phone', false).subscribe();

    const req = httpMock.expectOne(r => 
      r.url.includes('/api/v1/Product/ExportToExcel') &&
      r.url.includes('searchTerm=phone') &&
      r.url.includes('isActive=false')
    );
    expect(req.request.method).toBe('GET');
    expect(req.request.responseType).toBe('blob');
    req.flush(new Blob());
  });
});
