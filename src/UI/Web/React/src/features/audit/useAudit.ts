import { useQuery } from '@tanstack/react-query';
import { auditService } from '../../api/services';

export function useAudit() {
  const { data, isLoading } = useQuery({
    queryKey: ['audit-logs'],
    queryFn: () => auditService.getAuditLogs('Order'),
  });

  return {
    data,
    isLoading
  };
}
