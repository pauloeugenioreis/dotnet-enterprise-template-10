import { BaseService } from '../base.service';
import { AuditLogResponse } from '../../types';

export class AuditService extends BaseService<AuditLogResponse> {
  constructor() {
    super('/api/v1/audit');
  }

  /**
   * Sobrescreve o list para lidar com o endpoint específico por tipo de entidade
   */
  async getAuditLogs(page = 1, pageSize = 10, filters: { entityType?: string; eventType?: string; userId?: string; from?: string; toDate?: string } = {}) {
    const { entityType, ...otherFilters } = filters;
    const path = entityType ? `${this.entityPath}/${entityType}` : this.entityPath;
    const query = this.toQueryString({ page, pageSize, ...otherFilters });
    
    const response = await this.http.get(path + query);
    return response.data;
  }
}

export const auditService = new AuditService();
