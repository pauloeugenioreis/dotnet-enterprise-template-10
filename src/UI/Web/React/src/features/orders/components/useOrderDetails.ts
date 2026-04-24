import { orderService } from '../../../api/services';

export function useOrderDetails(onStatusUpdated: () => void) {
  const handleStatusChange = async (orderId: number, newStatus: string) => {
    try {
      await orderService.updateStatus(orderId, newStatus, 'Atualizado via Dashboard React');
      onStatusUpdated();
    } catch (error) {
      alert('Erro ao atualizar status');
    }
  };

  const getStatusClass = (status: string | undefined) => {
    switch (status?.toLowerCase()) {
      case 'pending': return 'bg-amber-50 text-amber-600';
      case 'completed': return 'bg-emerald-50 text-emerald-600';
      case 'cancelled': return 'bg-rose-50 text-rose-600';
      case 'shipped': return 'bg-indigo-50 text-indigo-600';
      default: return 'bg-gray-50 text-gray-600';
    }
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  };

  const formatDate = (date: string) => {
    return new Date(date).toLocaleString('pt-BR');
  };

  return {
    handleStatusChange,
    getStatusClass,
    formatCurrency,
    formatDate
  };
}
