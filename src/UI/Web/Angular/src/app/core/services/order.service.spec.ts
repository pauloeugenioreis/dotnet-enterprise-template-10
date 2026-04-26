import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { OrderService } from './order.service';
import { CreateOrderRequest } from '../../shared/models';

describe('OrderService', () => {
  let service: OrderService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        OrderService
      ]
    });
    service = TestBed.inject(OrderService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get orders with default parameters', () => {
    service.getOrders().subscribe();
    const req = httpMock.expectOne(r => r.url.includes('/api/v1/Order?page=1&pageSize=10'));
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get orders with custom filters', () => {
    const now = new Date();
    const from = new Date(now.getFullYear(), now.getMonth(), 1);
    const to = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    service.getOrders(1, 10, 'search', 'Pending', from, to).subscribe();

    const req = httpMock.expectOne(r => 
      r.url.includes('searchTerm=search') &&
      r.url.includes('status=Pending') &&
      r.url.includes(`startDate=${from.toISOString()}`) &&
      r.url.includes(`endDate=${to.toISOString()}`)
    );
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should create an order', () => {
    const mockOrder: CreateOrderRequest = { 
      customerName: 'John', 
      customerEmail: 'john@test.com',
      shippingAddress: 'Address 123',
      items: [{ productId: 'p1', quantity: 2, unitPrice: 10 }] 
    };
    service.create(mockOrder).subscribe();

    const req = httpMock.expectOne(r => r.url.includes('/api/v1/Order'));
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockOrder);
    req.flush({});
  });

  it('should update order status', () => {
    const orderId = '123';
    service.updateStatus(orderId, 'Shipped', 'Reason').subscribe();

    const req = httpMock.expectOne(r => r.url.endsWith(`/api/v1/Order/${orderId}/status`));
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ status: 'Shipped', reason: 'Reason' });
    req.flush({});
  });

  it('should export orders to excel', () => {
    service.exportToExcel('search', 'Completed').subscribe();

    const req = httpMock.expectOne(r => 
      r.url.includes('/api/v1/Order/ExportToExcel') &&
      r.url.includes('searchTerm=search') &&
      r.url.includes('status=Completed')
    );
    expect(req.request.method).toBe('GET');
    expect(req.request.responseType).toBe('blob');
    req.flush(new Blob());
  });

  it('should get statistics', () => {
    service.getStatistics().subscribe();

    const req = httpMock.expectOne(r => r.url.includes('/api/v1/Order/statistics'));
    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});
