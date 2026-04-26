export interface AuditLogResponse {
  eventId: string;
  eventType: string;
  aggregateType: string;
  aggregateId: string;
  occurredOn: string;
  userId: string;
  version: number;
}
