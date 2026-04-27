import { useInfiniteQuery } from '@tanstack/react-query';
import { productService } from '../../api/services/product.service';

export function useInfiniteProducts() {
  const {
    data,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
    isLoading
  } = useInfiniteQuery({
    queryKey: ['products-infinite'],
    queryFn: ({ pageParam = 1 }) => productService.list(pageParam as number, 100),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => {
      if (lastPage.items.length < 100) return undefined;
      return lastPage.page + 1;
    },
  });

  const products = data?.pages.flatMap((page) => page.items) || [];

  return {
    products,
    loadMore: fetchNextPage,
    hasMore: hasNextPage,
    isLoadingMore: isFetchingNextPage,
    isLoading
  };
}
