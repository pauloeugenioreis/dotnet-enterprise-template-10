import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { CustomerReviewService } from './customer-review.service';
import { PagedResponse, CustomerReviewResponse, CreateCustomerReviewRequest } from '../../shared/models';

describe('CustomerReviewService', () => {
  let service: CustomerReviewService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        CustomerReviewService
      ]
    });
    service = TestBed.inject(CustomerReviewService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get reviews with default parameters', () => {
    const mockResponse: PagedResponse<CustomerReviewResponse> = {
      items: [],
      page: 1,
      pageSize: 10,
      totalPages: 1,
      totalCount: 0,
      hasPreviousPage: false,
      hasNextPage: false
    };

    service.getReviews().subscribe(response => {
      expect(response).toEqual(mockResponse);
    });

    const req = httpMock.expectOne(r => r.url.includes('/api/v1/CustomerReview?page=1&pageSize=10'));
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should get reviews with custom parameters', () => {
    service.getReviews(2, 20, 'Product A', 4, true).subscribe();

    const req = httpMock.expectOne(r => 
      r.url.includes('/api/v1/CustomerReview?page=2&pageSize=20') &&
      r.url.includes('productName=Product%20A') &&
      r.url.includes('minRating=4') &&
      r.url.includes('isApproved=true')
    );
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get review by id', () => {
    const reviewId = '123';
    service.getById(reviewId).subscribe();

    const req = httpMock.expectOne(r => r.url.endsWith(`/api/v1/CustomerReview/${reviewId}`));
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should create a review', () => {
    const mockReview: CreateCustomerReviewRequest = {
      productName: 'Product A',
      customerName: 'John',
      rating: 5,
      title: 'Great!',
      comment: 'Excellent service',
      tags: ['tag1'],
      metadata: {},
      isVerifiedPurchase: true
    };
    service.create(mockReview).subscribe();

    const req = httpMock.expectOne(r => r.url.includes('/api/v1/CustomerReview'));
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockReview);
    req.flush({});
  });

  it('should update a review', () => {
    const reviewId = '123';
    const mockReview: CreateCustomerReviewRequest = {
      productName: 'Product A',
      customerName: 'John',
      rating: 4,
      title: 'Updated',
      comment: 'Still good',
      tags: [],
      metadata: {},
      isVerifiedPurchase: true
    };
    service.update(reviewId, mockReview).subscribe();

    const req = httpMock.expectOne(r => r.url.endsWith(`/api/v1/CustomerReview/${reviewId}`));
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(mockReview);
    req.flush({});
  });

  it('should approve a review', () => {
    const reviewId = '123';
    service.approve(reviewId, true).subscribe();

    const req = httpMock.expectOne(r => r.url.endsWith(`/api/v1/CustomerReview/${reviewId}/approve`));
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ isApproved: true });
    req.flush({});
  });

  it('should delete a review', () => {
    const reviewId = '123';
    service.delete(reviewId).subscribe();

    const req = httpMock.expectOne(r => r.url.endsWith(`/api/v1/CustomerReview/${reviewId}`));
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });
});
