import { useState } from 'react';
import { notify } from '../../utils/toast';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { orderService } from '../../api/services/order.service';
import { OrderResponse } from '../../types';
import { useInfiniteProducts } from '../products/useInfiniteProducts';

export function useOrders() {
  const { products, loadMore, hasMore, isLoadingMore } = useInfiniteProducts();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [status, setStatus] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['orders', page, pageSize, status, searchTerm, startDate, endDate],
    queryFn: () => orderService.list(page, pageSize, { status, searchTerm, startDate, endDate }),
  });

  const createMutation = useMutation({
    mutationFn: (order: any) => orderService.create(order),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orders'] });
      notify.success('Pedido Criado', 'O pedido foi registrado com sucesso.');
    },
  });

  const cancelMutation = useMutation({
    mutationFn: (id: number | string) => orderService.cancel(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orders'] });
      notify.success('Pedido Cancelado', 'O status do pedido foi atualizado.');
    },
  });

  const handleCancel = async (id: number) => {
    if (window.confirm('Tem certeza que deseja cancelar este pedido?')) {
      try {
        await cancelMutation.mutateAsync(id);
      } catch (error) {
        // Erro já tratado no interceptor global
      }
    }
  };

  const handleExport = async () => {
    try {
      const blob = await orderService.exportToExcel({ status, searchTerm, startDate, endDate });
      const url = window.URL.createObjectURL(new Blob([blob]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `Pedidos_${new Date().getTime()}.xlsx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      notify.success('Exportação Concluída', 'O arquivo Excel foi gerado.');
    } catch (error) {
      notify.error('Erro na Exportação', 'Não foi possível gerar o arquivo Excel.');
    }
  };

  const getStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'concluído':
      case 'delivered': return 'bg-green-100 text-green-700';
      case 'enviado':
      case 'shipped': return 'bg-blue-100 text-blue-700';
      case 'pendente':
      case 'pending': return 'bg-amber-100 text-amber-700';
      case 'cancelado':
      case 'cancelled': return 'bg-red-100 text-red-700';
      default: return 'bg-gray-100 text-gray-700';
    }
  };

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isDetailsOpen, setIsDetailsOpen] = useState(false);
  const [selectedOrder, setSelectedOrder] = useState<OrderResponse | null>(null);
  const [editingId, setEditingId] = useState<number | null>(null);

  const [formData, setFormData] = useState({
    customerName: '',
    customerEmail: '',
    shippingAddress: '',
    items: [] as any[]
  });

  const handleSubmit = async () => {
    try {
      await createMutation.mutateAsync(formData);
      setIsModalOpen(false);
      setFormData({ customerName: '', customerEmail: '', shippingAddress: '', items: [] });
    } catch (error) {
      // Erro tratado no interceptor
    }
  };

  const handleEdit = (order: any) => {
    setEditingId(order.id);
    setFormData({
      customerName: order.customerName || '',
      customerEmail: order.customerEmail || '',
      shippingAddress: order.shippingAddress || '',
      items: order.items || []
    });
    setIsModalOpen(true);
  };

  const handleAddItem = () => {
    setFormData({
      ...formData,
      items: [...formData.items, { productId: '', quantity: 1, unitPrice: 0 }]
    });
  };

  const handleRemoveItem = (index: number) => {
    setFormData({
      ...formData,
      items: formData.items.filter((_, i) => i !== index)
    });
  };

  const handleItemChange = (index: number, field: string, value: any) => {
    const newItems = [...formData.items];
    if (field === 'productId') {
      const product = products.find((p: any) => p.id === parseInt(value));
      newItems[index] = {
        ...newItems[index],
        productId: parseInt(value),
        unitPrice: product?.price || 0
      };
    } else {
      newItems[index] = { ...newItems[index], [field]: value };
    }
    setFormData({ ...formData, items: newItems });
  };

  const handleOpenNew = () => {
    setEditingId(null);
    setFormData({ customerName: '', customerEmail: '', shippingAddress: '', items: [] });
    setIsModalOpen(true);
  };

  const handleViewDetails = (order: OrderResponse) => {
    setSelectedOrder(order);
    setIsDetailsOpen(true);
  };

  return {
    data,
    isLoading,
    getStatusColor,
    page,
    setPage,
    pageSize,
    setPageSize: (size: number) => {
      setPageSize(size);
      setPage(1);
    },
    status,
    setStatus: (s: string) => {
      setStatus(s);
      setPage(1);
    },
    searchTerm,
    setSearchTerm: (term: string) => {
      setSearchTerm(term);
      setPage(1);
    },
    startDate,
    setStartDate: (date: string) => {
      setStartDate(date);
      setPage(1);
    },
    endDate,
    setEndDate: (date: string) => {
      setEndDate(date);
      setPage(1);
    },
    handleCancel,
    handleExport,
    createOrder: createMutation.mutateAsync,
    isCreating: createMutation.isPending,
    
    // Infinite Scroll Data
    products,
    loadMoreProducts: loadMore,
    hasMoreProducts: hasMore,
    isProductsLoading: isLoadingMore,
    
    // Modal & Form State
    isModalOpen,
    setIsModalOpen,
    isDetailsOpen,
    setIsDetailsOpen,
    selectedOrder,
    setSelectedOrder,
    editingId,
    formData,
    setFormData,
    
    // Handlers
    handleSubmit,
    handleEdit,
    handleAddItem,
    handleRemoveItem,
    handleItemChange,
    handleOpenNew,
    handleViewDetails
  };
}
