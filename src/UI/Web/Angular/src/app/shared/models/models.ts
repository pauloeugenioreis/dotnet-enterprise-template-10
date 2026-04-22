// Espelho dos DTOs do C# (src/Shared/SharedModels)

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface AuthResponseDto {
  token: string;
  userId: string;
  email: string;
  name: string;
}

export interface OrderResponseDto {
  id: string;
  orderNumber: string;
  customerName: string;
  status: string;
  total: number;
  createdAt: string;
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
