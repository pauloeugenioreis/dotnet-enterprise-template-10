import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { orderService } from '../../api/services';

export function useOrders() {
  const [page, setPage] = useState(1);
  const [pageSize] = useState(5);
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['orders', page, pageSize],
    queryFn: () => orderService.getOrders(page, pageSize),
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
      const response = await orderService.exportToExcel();
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
      case 'delivered': return 'bg-green-100 text-green-700';
      case 'shipped': return 'bg-blue-100 text-blue-700';
      case 'pending': return 'bg-amber-100 text-amber-700';
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
    handleCancel,
    handleExport,
    createOrder: createMutation.mutateAsync,
    isCreating: createMutation.isPending
  };
}
