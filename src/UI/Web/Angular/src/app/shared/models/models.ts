// Espelho dos DTOs do C# (src/Shared/Shared.Models)

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface User {
  id: string | number;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  profileImageUrl: string | null;
  roles: string[];
  createdAt: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export interface OrderItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface OrderResponse {
  id: string;
  orderNumber: string;
  customerName: string;
  customerEmail?: string;
  shippingAddress?: string;
  status: string;
  subtotal: number;
  shippingCost: number;
  tax: number;
  total: number;
  createdAt: string;
  items?: OrderItem[];
}

export interface OrderStatistics {
  totalOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
  topProducts: any[];
}

export interface DomainEvent {
  eventId: string;
  eventType: string;
  aggregateType: string;
  aggregateId: string;
  occurredOn: string;
  userId: string;
  version: number;
}

export interface ProductResponse {
  id: string;
  name: string;
  category: string;
  price: number;
  stock: number;
  isActive: boolean;
}

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
