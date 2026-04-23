import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { orderService } from '../../api/services';

export function useOrders() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [status, setStatus] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['orders', page, pageSize, status, searchTerm, startDate, endDate],
    queryFn: () => orderService.getOrders(page, pageSize, status, searchTerm, startDate, endDate),
  });

  const createMutation = useMutation({
    mutationFn: (order: any) => orderService.createOrder(order),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['orders'] }),
  });

  const cancelMutation = useMutation({
    mutationFn: (id: number) => orderService.deleteOrder(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['orders'] }),
  });

  const handleCancel = async (id: number) => {
    if (window.confirm('Tem certeza que deseja cancelar este pedido?')) {
      try {
        await cancelMutation.mutateAsync(id);
      } catch (error) {
        alert('Erro ao cancelar pedido');
      }
    }
  };

  const handleExport = async () => {
    try {
      const response = await orderService.exportToExcel(status, searchTerm, startDate, endDate);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `Pedidos_${new Date().getTime()}.xlsx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (error) {
      alert('Erro ao exportar pedidos');
    }
  };

  const getStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'concluído':
      case 'delivered': return 'bg-green-100 text-green-700';
      case 'enviado':
      case 'shipped': return 'bg-blue-100 text-blue-700';
      case 'pendente':
      case 'pending': return 'bg-amber-100 text-amber-700';
      case 'cancelado':
      case 'cancelled': return 'bg-red-100 text-red-700';
      default: return 'bg-gray-100 text-gray-700';
    }
  };

  return {
    data,
    isLoading,
    getStatusColor,
    page,
    setPage,
    pageSize,
    setPageSize: (size: number) => {
      setPageSize(size);
      setPage(1);
    },
    status,
    setStatus: (s: string) => {
      setStatus(s);
      setPage(1);
    },
    searchTerm,
    setSearchTerm: (term: string) => {
      setSearchTerm(term);
      setPage(1);
    },
    startDate,
    setStartDate: (date: string) => {
      setStartDate(date);
      setPage(1);
    },
    endDate,
    setEndDate: (date: string) => {
      setEndDate(date);
      setPage(1);
    },
    handleCancel,
    handleExport,
    createOrder: createMutation.mutateAsync,
    isCreating: createMutation.isPending
  };
}
