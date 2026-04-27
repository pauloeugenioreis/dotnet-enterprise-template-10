import { BaseService } from './BaseService';
import { OrderResponse, CreateOrderRequest, UpdateOrderRequest, OrderStatistics } from '../../types';

export class OrderService extends BaseService<OrderResponse, CreateOrderRequest, UpdateOrderRequest> {
  constructor() {
    super('/api/v1/order');
  }

  async updateStatus(id: number | string, status: string, note: string = 'Atualizado via Vue Dashboard'): Promise<void> {
    await this.http.put(`${this.entityPath}/${id}/status`, { status, note });
  }

  async exportToExcel(filters: Record<string, any> = {}): Promise<void> {
    const query = this.toQueryString(filters);
    await this.downloadFile(`${this.entityPath}/ExportToExcel${query}`, `Pedidos_${new Date().getTime()}.xlsx`);
  }

  async getStatistics(): Promise<OrderStatistics> {
    const response = await this.http.get<OrderStatistics>(`${this.entityPath}/statistics`);
    return response.data;
  }

  async cancel(id: number | string): Promise<void> {
    await this.http.post(`${this.entityPath}/${id}/cancel`);
  }
}

export const orderService = new OrderService();
