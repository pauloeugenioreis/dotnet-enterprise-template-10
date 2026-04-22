import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { PagedResponse, OrderResponseDto, DomainEvent } from '../../shared/models/models';

@Injectable({ providedIn: 'root' })
export class OrderService extends BaseService {
  getOrders(page = 1, pageSize = 10) {
    return this.http.get<PagedResponse<OrderResponseDto>>(`${this.baseUrl}/api/orders?page=${page}&pageSize=${pageSize}`);
  }
}

@Injectable({ providedIn: 'root' })
export class ProductService extends BaseService {
  getProducts(page = 1, pageSize = 10) {
    return this.http.get<PagedResponse<any>>(`${this.baseUrl}/api/products?page=${page}&pageSize=${pageSize}`);
  }
}

@Injectable({ providedIn: 'root' })
export class AuditService extends BaseService {
  getAuditLogs(entityType: string, page = 1, pageSize = 10) {
    return this.http.get<PagedResponse<DomainEvent>>(`${this.baseUrl}/api/v1/audit/${entityType}?page=${page}&pageSize=${pageSize}`);
  }
}
