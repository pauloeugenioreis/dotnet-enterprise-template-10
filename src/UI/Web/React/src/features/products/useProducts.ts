import { useQuery } from '@tanstack/react-query';
import { productService } from '../../api/services';

export function useProducts() {
  const { data, isLoading } = useQuery({
    queryKey: ['products'],
    queryFn: () => productService.getProducts(),
  });

  return {
    data,
    isLoading
  };
}
