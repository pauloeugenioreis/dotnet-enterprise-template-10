import { BaseService } from './BaseService';
import api from '../api';

export class OrderService extends BaseService<any> {
  constructor() {
    super('/api/v1/order');
  }

  async updateStatus(id: number, status: string, note: string): Promise<void> {
    await api.put(`${this.resourcePath}/${id}/status`, { status, note });
  }

  async exportToExcel(): Promise<void> {
    await this.downloadFile(`${this.resourcePath}/ExportToExcel`, `Pedidos_${new Date().getTime()}.xlsx`);
  }
}

export const orderService = new OrderService();
