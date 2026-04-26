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

export interface CreateCustomerReviewRequest {
  productName: string;
  customerName: string;
  customerEmail?: string;
  rating: number;
  title: string;
  comment?: string;
  tags: string[];
  metadata: Record<string, string>;
  isVerifiedPurchase: boolean;
}

export interface ApproveCustomerReviewRequest {
  isApproved: boolean;
}
