import { ref } from 'vue';
import { useToast } from 'vue-toastification';
import { orderService } from '../../api/services/OrderService';
import { OrderResponse, PagedResponse, OrderStatistics } from '../../types';

export function useOrders() {
  const orders = ref<OrderResponse[]>([]);
  const totalCount = ref(0);
  const loading = ref(false);
  const statistics = ref<OrderStatistics | null>(null);
  const toast = useToast();

  const fetchOrders = async (page = 1, pageSize = 10, filters = {}) => {
    loading.value = true;
    try {
      const data = await orderService.list(page, pageSize, filters);
      orders.value = data.items;
      totalCount.value = data.totalCount;
      return data;
    } finally {
      loading.value = false;
    }
  };

  const fetchStatistics = async () => {
    try {
      statistics.value = await orderService.getStatistics();
    } catch (error) {
      console.error('Erro ao buscar estatísticas', error);
    }
  };

  const cancelOrder = async (id: number | string) => {
    loading.value = true;
    try {
      await orderService.cancel(id);
      toast.success('Pedido cancelado com sucesso.');
    } finally {
      loading.value = false;
    }
  };

  return {
    orders,
    totalCount,
    loading,
    statistics,
    fetchOrders,
    fetchStatistics,
    cancelOrder
  };
}
