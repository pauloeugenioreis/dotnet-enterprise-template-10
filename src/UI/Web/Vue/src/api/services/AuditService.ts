import { BaseService } from './BaseService';
import { AuditLogResponse } from '../../types';

export class AuditService extends BaseService<AuditLogResponse> {
  constructor() {
    super('/api/v1/audit');
  }

  async getAuditLogs(page = 1, pageSize = 10, filters: Record<string, any> = {}): Promise<any> {
    const { entityType, ...otherFilters } = filters;
    const path = entityType ? `${this.entityPath}/${entityType}` : this.entityPath;
    const query = this.toQueryString({ page, pageSize, ...otherFilters });
    
    const response = await this.http.get(path + query);
    return response.data;
  }
}

export const auditService = new AuditService();
