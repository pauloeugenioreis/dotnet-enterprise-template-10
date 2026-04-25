import { useQuery } from '@tanstack/react-query';
import { orderService } from '../../api/services';

export function useDashboard() {
  const { data: orders, isLoading: ordersLoading } = useQuery({
    queryKey: ['orders-latest'],
    queryFn: () => orderService.getOrders(1, 5),
  });

  const { data: statistics, isLoading: statsLoading } = useQuery({
    queryKey: ['orders-stats'],
    queryFn: () => orderService.getStatistics(),
  });

  const stats = [
    { 
      label: 'Vendas Totais', 
      value: statistics ? new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(statistics.totalRevenue) : '...', 
      icon: '📈', 
      color: 'bg-blue-50' 
    },
    { 
      label: 'Total de Pedidos', 
      value: statistics ? statistics.totalOrders.toString() : '...', 
      icon: '👤', 
      color: 'bg-purple-50' 
    },
    { 
      label: 'Status do Sistema', 
      value: 'Operacional', 
      icon: '✅', 
      color: 'bg-green-50' 
    }
  ];

  return {
    orders,
    isLoading: ordersLoading || statsLoading,
    stats
  };
}
