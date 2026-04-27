import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { PagedResponse, OrderResponse, OrderStatistics, CreateOrderRequest, UpdateOrderRequest } from '../../shared/models';

@Injectable({ providedIn: 'root' })
export class OrderService extends BaseService<OrderResponse, CreateOrderRequest, UpdateOrderRequest> {
  protected override entityPath = 'api/v1/Order';

  getOrders(page = 1, pageSize = 10, search = '', status = '', from?: Date, to?: Date) {
    let url = `${this.fullUrl}?page=${page}&pageSize=${pageSize}`;
    if (search) url += `&searchTerm=${encodeURIComponent(search)}`;
    if (status) url += `&status=${status}`;
    if (from) url += `&startDate=${from.toISOString()}`;
    if (to) url += `&endDate=${to.toISOString()}`;
    return this.http.get<PagedResponse<OrderResponse>>(url);
  }

  updateStatus(id: string, status: string, reason: string = 'Atualizado via Dashboard') {
    return this.http.patch(`${this.fullUrl}/${id}/status`, { status, reason });
  }

  exportToExcel(search = '', status = '', from?: Date, to?: Date) {
    let url = `${this.fullUrl}/ExportToExcel?`;
    if (search) url += `&searchTerm=${encodeURIComponent(search)}`;
    if (status) url += `&status=${status}`;
    if (from) url += `&startDate=${from.toISOString()}`;
    if (to) url += `&endDate=${to.toISOString()}`;
    return this.http.get(url, { responseType: 'blob' });
  }

  getStatistics() {
    return this.http.get<OrderStatistics>(`${this.fullUrl}/statistics`);
  }
}
