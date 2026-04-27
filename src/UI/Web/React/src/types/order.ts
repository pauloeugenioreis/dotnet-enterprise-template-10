export interface OrderItem {
  productId: string | number;
  productName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface OrderResponse {
  id: string | number;
  orderNumber: string;
  customerName: string;
  customerEmail: string;
  shippingAddress: string;
  status: string;
  subtotal: number;
  shippingCost: number;
  tax: number;
  total: number;
  createdAt: string;
  items: OrderItem[];
}

export interface CreateOrderRequest {
  customerName: string;
  customerEmail: string;
  shippingAddress: string;
  items: {
    productId: string | number;
    quantity: number;
    unitPrice: number;
  }[];
}

export interface UpdateOrderRequest {
  customerName: string;
  status: string;
  shippingAddress?: string;
  notes?: string;
}

export interface OrderStatistics {
  totalOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
  topProducts: {
    productId: string;
    productName: string;
    totalQuantity: number;
    totalRevenue: number;
  }[];
}
