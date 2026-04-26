import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { orderService } from '../../api/services/order.service';
import { CreateOrderRequest } from '../../types';

export const useOrders = (page = 1, pageSize = 10, filters = {}) => {
  return useQuery({
    queryKey: ['orders', page, pageSize, filters],
    queryFn: () => orderService.list(page, pageSize, filters),
  });
};

export const useOrderStatistics = () => {
  return useQuery({
    queryKey: ['order-statistics'],
    queryFn: () => orderService.getStatistics(),
  });
};

export const useCreateOrder = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (order: CreateOrderRequest) => orderService.create(order),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orders'] });
      queryClient.invalidateQueries({ queryKey: ['order-statistics'] });
    },
  });
};

export const useUpdateOrderStatus = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status, reason }: { id: string | number; status: string; reason?: string }) => 
      orderService.updateStatus(id, status, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orders'] });
    },
  });
};
