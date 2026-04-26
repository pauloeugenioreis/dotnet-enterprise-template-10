import { BaseService } from './BaseService';
import { CustomerReviewResponse, CreateCustomerReviewRequest } from '../../types';

export class CustomerReviewService extends BaseService<CustomerReviewResponse, CreateCustomerReviewRequest, any> {
  constructor() {
    super('/api/v1/customerreview');
  }

  async approve(id: string | number, isApproved: boolean): Promise<void> {
    await this.http.patch(`${this.entityPath}/${id}/approve`, { isApproved });
  }
}

export const customerReviewService = new CustomerReviewService();
