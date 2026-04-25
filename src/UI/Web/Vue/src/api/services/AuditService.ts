import { BaseService } from './BaseService';

export class AuditService extends BaseService<any> {
  constructor() {
    super('/api/v1/audit');
  }
}

export const auditService = new AuditService();
