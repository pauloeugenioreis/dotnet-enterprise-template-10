import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { PagedResponse, OrderResponseDto, DomainEvent } from '../../shared/models/models';

@Injectable({ providedIn: 'root' })
export class OrderService extends BaseService {
  getOrders(page = 1, pageSize = 10, search = '', status = '', from?: Date, to?: Date) {
    let url = `${this.baseUrl}/api/v1/Order?page=${page}&pageSize=${pageSize}`;
    if (search) url += `&searchTerm=${encodeURIComponent(search)}`;
    if (status) url += `&status=${status}`;
    if (from) url += `&startDate=${from.toISOString()}`;
    if (to) url += `&endDate=${to.toISOString()}`;
    return this.http.get<PagedResponse<OrderResponseDto>>(url);
  }

  createOrder(order: any) {
    return this.http.post(`${this.baseUrl}/api/v1/Order`, order);
  }

  updateOrder(id: string, order: any) {
    return this.http.put(`${this.baseUrl}/api/v1/Order/${id}`, order);
  }

  updateStatus(id: string, status: string, reason: string = 'Atualizado via Dashboard') {
    return this.http.patch(`${this.baseUrl}/api/v1/Order/${id}/status`, { status, reason });
  }

  exportToExcel(search = '', status = '', from?: Date, to?: Date) {
    let url = `${this.baseUrl}/api/v1/Order/ExportToExcel?`;
    if (search) url += `&searchTerm=${encodeURIComponent(search)}`;
    if (status) url += `&status=${status}`;
    if (from) url += `&startDate=${from.toISOString()}`;
    if (to) url += `&endDate=${to.toISOString()}`;
    return this.http.get(url, { responseType: 'blob' });
  }
}

@Injectable({ providedIn: 'root' })
export class ProductService extends BaseService {
  getProducts(page = 1, pageSize = 10, search = '', isActive?: boolean) {
    let url = `${this.baseUrl}/api/v1/Product?page=${page}&pageSize=${pageSize}`;
    if (search) url += `&searchTerm=${encodeURIComponent(search)}`;
    if (isActive !== undefined) url += `&isActive=${isActive}`;
    return this.http.get<PagedResponse<any>>(url);
  }

  createProduct(product: any) {
    return this.http.post(`${this.baseUrl}/api/v1/Product`, product);
  }

  updateProduct(id: string, product: any) {
    return this.http.put(`${this.baseUrl}/api/v1/Product/${id}`, product);
  }

  deleteProduct(id: string) {
    return this.http.delete(`${this.baseUrl}/api/v1/Product/${id}`);
  }

  exportToExcel(search = '', isActive?: boolean) {
    let url = `${this.baseUrl}/api/v1/Product/ExportToExcel?`;
    if (search) url += `&searchTerm=${encodeURIComponent(search)}`;
    if (isActive !== undefined) url += `&isActive=${isActive}`;
    return this.http.get(url, { responseType: 'blob' });
  }
}

@Injectable({ providedIn: 'root' })
export class AuditService extends BaseService {
  getAuditLogs(page = 1, pageSize = 10, entityType?: string, eventType?: string, userId?: string, from?: Date, to?: Date) {
    let url = `${this.baseUrl}/api/v1/audit?page=${page}&pageSize=${pageSize}`;
    if (entityType) url += `&entityType=${entityType}`;
    if (eventType) url += `&eventType=${encodeURIComponent(eventType)}`;
    if (userId) url += `&userId=${encodeURIComponent(userId)}`;
    if (from) url += `&startDate=${from.toISOString()}`;
    if (to) url += `&endDate=${to.toISOString()}`;
    return this.http.get<PagedResponse<DomainEvent>>(url);
  }
}
