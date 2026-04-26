import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { productService } from '../../api/services/product.service';
import { CreateProductRequest } from '../../types';

export const useProducts = (page = 1, pageSize = 10, filters = {}) => {
  return useQuery({
    queryKey: ['products', page, pageSize, filters],
    queryFn: () => productService.list(page, pageSize, filters),
  });
};

export const useCreateProduct = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (product: CreateProductRequest) => productService.create(product),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
  });
};

export const useUpdateProduct = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, product }: { id: string | number; product: any }) => 
      productService.update(id, product),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
  });
};

export const useDeleteProduct = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string | number) => productService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
    },
  });
};
