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
    window.open(`${import.meta.env.VITE_API_BASE_URL || 'https://localhost:7196'}${this.resourcePath}/export`, '_blank');
  }
}

export const orderService = new OrderService();
