import { useQuery } from '@tanstack/react-query';
import { orderService } from '../../api/services';

export function useDashboard() {
  const { data: orders, isLoading } = useQuery({
    queryKey: ['orders-stats'],
    queryFn: () => orderService.getOrders(1, 5),
  });

  const stats = [
    { label: 'Vendas Mensais', value: 'R$ 842.500', icon: '📈', color: 'bg-blue-50' },
    { label: 'Novos Clientes', value: '+ 128', icon: '👤', color: 'bg-purple-50' },
    { label: 'Status do Sistema', value: 'Operacional', icon: '✅', color: 'bg-green-50' }
  ];

  return {
    orders,
    isLoading,
    stats
  };
}
