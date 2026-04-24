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

export interface UserDto {
  id: string | number;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  profileImageUrl: string | null;
  roles: string[];
  createdAt: string;
}

export interface AuthResponseDto {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserDto;
}

export interface OrderItemDto {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface OrderResponseDto {
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
  items?: OrderItemDto[];
}

export interface OrderStatisticsDto {
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

export interface ProductResponseDto {
  id: string;
  name: string;
  category: string;
  price: number;
  stock: number;
  isActive: boolean;
}
