import { useState } from 'react';
import { notify } from '../../utils/toast';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { productService } from '../../api/services/product.service';

export function useProducts() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [searchTerm, setSearchTerm] = useState('');
  const [isActive, setIsActive] = useState<boolean | undefined>(undefined);
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['products', page, pageSize, searchTerm, isActive],
    queryFn: () => productService.list(page, pageSize, { searchTerm, isActive }),
  });

  const createMutation = useMutation({
    mutationFn: (product: any) => productService.create(product),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      notify.success('Produto Criado', 'O produto foi adicionado ao catálogo.');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, product }: { id: number, product: any }) => productService.update(id, product),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      notify.success('Produto Atualizado', 'As alterações foram salvas.');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => productService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      notify.success('Produto Excluído', 'O produto foi removido permanentemente.');
    },
  });

  const deleteProduct = async (id: number) => {
    try {
      await deleteMutation.mutateAsync(id);
    } catch (error) {
      // Erro já tratado no interceptor
    }
  };

  const handleExport = async () => {
    try {
      const blob = await productService.exportToExcel({ searchTerm, isActive });
      const url = window.URL.createObjectURL(new Blob([blob]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `Produtos_${new Date().getTime()}.xlsx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      notify.success('Exportação Concluída', 'O arquivo Excel foi gerado.');
    } catch (error) {
      notify.error('Erro na Exportação', 'Não foi possível gerar o arquivo Excel.');
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
    deleteProduct,
    handleExport,
    createProduct: createMutation.mutateAsync,
    updateProduct: updateMutation.mutateAsync,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending
  };
}
