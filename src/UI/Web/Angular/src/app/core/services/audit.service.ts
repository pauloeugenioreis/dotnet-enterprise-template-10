import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { PagedResponse, DomainEvent } from '../../shared/models';

@Injectable({ providedIn: 'root' })
export class AuditService extends BaseService<DomainEvent> {
  protected override entityPath = 'api/v1/audit';

  getAuditLogs(page = 1, pageSize = 10, entityType?: string, eventType?: string, userId?: string, from?: Date, to?: Date) {
    let url = `${this.fullUrl}?page=${page}&pageSize=${pageSize}`;
    if (entityType) url += `&entityType=${entityType}`;
    if (eventType) url += `&eventType=${encodeURIComponent(eventType)}`;
    if (userId) url += `&userId=${encodeURIComponent(userId)}`;
    if (from) url += `&startDate=${from.toISOString()}`;
    if (to) url += `&endDate=${to.toISOString()}`;
    return this.http.get<PagedResponse<DomainEvent>>(url);
  }
}
