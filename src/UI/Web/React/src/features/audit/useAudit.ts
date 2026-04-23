import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { auditService } from '../../api/services';

export function useAudit() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [entityType, setEntityType] = useState('');
  const [eventType, setEventType] = useState('');
  const [userId, setUserId] = useState('');
  const [from, setFrom] = useState('');
  const [toDate, setToDate] = useState('');

  const { data, isLoading } = useQuery({
    queryKey: ['audit-logs', page, pageSize, entityType, eventType, userId, from, toDate],
    queryFn: () => auditService.getAuditLogs(page, pageSize, entityType, eventType, userId, from, toDate),
  });

  return {
    data,
    isLoading,
    page,
    setPage,
    pageSize,
    setPageSize: (size: number) => {
      setPageSize(size);
      setPage(1);
    },
    entityType,
    setEntityType: (type: string) => {
      setEntityType(type);
      setPage(1);
    },
    eventType,
    setEventType: (type: string) => {
      setEventType(type);
      setPage(1);
    },
    userId,
    setUserId: (id: string) => {
      setUserId(id);
      setPage(1);
    },
    from,
    setFrom: (date: string) => {
      setFrom(date);
      setPage(1);
    },
    toDate,
    setToDate: (date: string) => {
      setToDate(date);
      setPage(1);
    }
  };
}
