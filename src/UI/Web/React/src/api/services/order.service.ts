import { BaseService } from '../base.service';
import { OrderResponse, CreateOrderRequest, UpdateOrderRequest, OrderStatistics } from '../../types';

export class OrderService extends BaseService<OrderResponse, CreateOrderRequest, UpdateOrderRequest> {
  constructor() {
    super('/api/v1/Order');
  }

  async updateStatus(id: string | number, status: string, reason: string = 'Atualizado via Dashboard') {
    return this.http.patch(`${this.entityPath}/${id}/status`, { status, reason });
  }

  async cancel(id: string | number) {
    return this.http.post(`${this.entityPath}/${id}/cancel`);
  }

  async getStatistics(): Promise<OrderStatistics> {
    const response = await this.http.get<OrderStatistics>(`${this.entityPath}/statistics`);
    return response.data;
  }

  async exportToExcel(filters: Record<string, any> = {}) {
    const query = this.toQueryString(filters);
    const response = await this.http.get(`${this.entityPath}/ExportToExcel${query}`, {
      responseType: 'blob'
    });
    return response.data;
  }
}

export const orderService = new OrderService();
