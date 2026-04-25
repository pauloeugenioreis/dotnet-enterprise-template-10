import api from '../api';

export interface CustomerReviewResponse {
  id: string;
  productName: string;
  customerName: string;
  customerEmail?: string;
  rating: number;
  title: string;
  comment?: string;
  tags: string[];
  metadata: Record<string, string>;
  isVerifiedPurchase: boolean;
  isApproved: boolean;
  createdAt: string;
  updatedAt?: string;
}

export class CustomerReviewService {
  private resourcePath = '/api/v1/CustomerReview';

  async getReviews(page = 1, pageSize = 10, productName?: string, minRating?: number, isApproved?: boolean) {
    let url = `${this.resourcePath}?page=${page}&pageSize=${pageSize}`;
    if (productName) url += `&productName=${encodeURIComponent(productName)}`;
    if (minRating) url += `&minRating=${minRating}`;
    if (isApproved !== undefined) url += `&isApproved=${isApproved}`;
    const response = await api.get(url);
    return response.data;
  }

  async getById(id: string): Promise<CustomerReviewResponse> {
    const response = await api.get(`${this.resourcePath}/${id}`);
    return response.data;
  }

  async create(review: any): Promise<CustomerReviewResponse> {
    const response = await api.post(this.resourcePath, review);
    return response.data;
  }

  async update(id: string, review: any): Promise<void> {
    await api.put(`${this.resourcePath}/${id}`, review);
  }

  async approve(id: string, isApproved: boolean): Promise<void> {
    await api.patch(`${this.resourcePath}/${id}/approve`, { isApproved });
  }

  async delete(id: string): Promise<void> {
    await api.delete(`${this.resourcePath}/${id}`);
  }
}

export const customerReviewService = new CustomerReviewService();
