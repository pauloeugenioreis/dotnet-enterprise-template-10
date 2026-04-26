import { BaseService } from '../base.service';
import { CustomerReviewResponse, CreateCustomerReviewRequest } from '../../types';

export class CustomerReviewService extends BaseService<CustomerReviewResponse, CreateCustomerReviewRequest> {
  constructor() {
    super('/api/v1/CustomerReview');
  }

  async approve(id: string | number, isApproved: boolean) {
    return this.http.patch(`${this.entityPath}/${id}/approve`, { isApproved });
  }
}

export const customerReviewService = new CustomerReviewService();
