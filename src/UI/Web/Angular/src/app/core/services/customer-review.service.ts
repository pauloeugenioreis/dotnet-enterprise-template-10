import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { PagedResponse, CustomerReviewResponse, CreateCustomerReviewRequest, ApproveCustomerReviewRequest } from '../../shared/models';

@Injectable({ providedIn: 'root' })
export class CustomerReviewService extends BaseService<CustomerReviewResponse, CreateCustomerReviewRequest> {
  protected override entityPath = 'api/v1/CustomerReview';

  getReviews(page = 1, pageSize = 10, productName = '', minRating?: number, isApproved?: boolean) {
    let url = `${this.fullUrl}?page=${page}&pageSize=${pageSize}`;
    if (productName) url += `&productName=${encodeURIComponent(productName)}`;
    if (minRating) url += `&minRating=${minRating}`;
    if (isApproved !== undefined) url += `&isApproved=${isApproved}`;
    return this.http.get<PagedResponse<CustomerReviewResponse>>(url);
  }

  approve(id: string, isApproved: boolean) {
    return this.http.patch(`${(this as any).fullUrl}/${id}/approve`, { isApproved } as ApproveCustomerReviewRequest);
  }
}
