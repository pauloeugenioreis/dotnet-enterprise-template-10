import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { productService } from '../../api/services';

export function useProducts() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [searchTerm, setSearchTerm] = useState('');
  const [isActive, setIsActive] = useState<boolean | undefined>(undefined);
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['products', page, pageSize, searchTerm, isActive],
    queryFn: () => productService.getProducts(page, pageSize, searchTerm, isActive),
  });

  const createMutation = useMutation({
    mutationFn: (product: any) => productService.createProduct(product),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['products'] }),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, product }: { id: number, product: any }) => productService.updateProduct(id, product),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['products'] }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => productService.deleteProduct(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['products'] }),
  });

  const handleDelete = async (id: number) => {
    if (window.confirm('Tem certeza que deseja excluir este produto?')) {
      try {
        await deleteMutation.mutateAsync(id);
      } catch (error) {
        alert('Erro ao excluir produto');
      }
    }
  };

  const handleExport = async () => {
    try {
      const response = await productService.exportToExcel(searchTerm, isActive);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `Produtos_${new Date().getTime()}.xlsx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (error) {
      alert('Erro ao exportar produtos');
    }
  };

  return {
    data,
    isLoading,
    page,
    setPage,
    pageSize,
    setPageSize: (size: number) => {
      setPageSize(size);
      setPage(1);
    },
    searchTerm,
    setSearchTerm: (term: string) => {
      setSearchTerm(term);
      setPage(1);
    },
    isActive,
    setIsActive: (active: boolean | undefined) => {
      setIsActive(active);
      setPage(1);
    },
    handleDelete,
    handleExport,
    createProduct: createMutation.mutateAsync,
    updateProduct: updateMutation.mutateAsync,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending
  };
}
