<template src="./Orders.html"></template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue';
import { useOrders } from '../../composables/api/useOrders';
import { useProducts } from '../../composables/api/useProducts';
import { orderService } from '../../api/services/OrderService';
import { productService } from '../../api/services/ProductService';
import Dropdown from '../../components/Dropdown.vue';
import Pagination from '../../components/Pagination.vue';
import Modal from '../../components/Modal.vue';
import Drawer from '../../components/Drawer.vue';
import OrderDetailsModal from './components/OrderDetailsModal.vue';
import ConfirmModal from '../../components/ConfirmModal.vue';
import { useToast } from 'vue-toastification';

const toast = useToast();

const { 
  orders, 
  loading, 
  totalCount, 
  fetchOrders, 
  cancelOrder 
} = useOrders();

const { 
  products, 
  fetchProducts: fetchAllProducts 
} = useProducts();

const page = ref(1);
const pageSize = ref(10);
const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value));
const searchTerm = ref('');
const status = ref('');
const startDate = ref('');
const endDate = ref('');

const statusOptions = [
  { label: 'Todos os Status', value: '' },
  { label: 'Pendente', value: 'Pending' },
  { label: 'Enviado', value: 'Shipped' },
  { label: 'Entregue', value: 'Delivered' },
  { label: 'Cancelado', value: 'Cancelled' }
];

const isModalOpen = ref(false);
const isDetailsOpen = ref(false);
const selectedOrder = ref<any>(null);

const productPage = ref(1);
const productLoading = ref(false);
const hasMoreProducts = ref(true);

const handleLoadMoreProducts = () => {
  if (hasMoreProducts.value) {
    fetchAllProducts(productPage.value + 1, 100);
  }
};

const editingId = ref<number | null>(null);
const formData = ref({
  customerName: '',
  customerEmail: '',
  shippingAddress: '',
  items: [] as any[]
});

const productOptions = computed(() => 
  (products.value || []).map(p => ({
    label: p ? `${p.name} - R$ ${p.price}` : '',
    value: p?.id?.toString() || ''
  }))
);

const handleFetchOrders = () => {
  fetchOrders(page.value, pageSize.value, {
    searchTerm: searchTerm.value,
    status: status.value,
    startDate: startDate.value,
    endDate: endDate.value
  });
};

const fetchProductsList = () => fetchAllProducts(1, 100);

const handleExport = () => orderService.exportToExcel();

const handleOpenNew = () => {
  editingId.value = null;
  formData.value = { customerName: '', customerEmail: '', shippingAddress: '', items: [] };
  isModalOpen.value = true;
};

const handleEdit = async (order: any) => {
  try {
    const fullOrder = await orderService.getById(order.id);
    editingId.value = fullOrder.id as any;
    formData.value = {
      customerName: fullOrder.customerName || '',
      customerEmail: fullOrder.customerEmail || '',
      shippingAddress: fullOrder.shippingAddress || '',
      items: fullOrder.items ? fullOrder.items.map((item: any) => ({
        productId: item.productId,
        quantity: item.quantity,
        unitPrice: item.unitPrice
      })) : []
    };
    fetchProductsList();
    isModalOpen.value = true;
  } catch (error) {
    // Erro tratado pelo interceptor
  }
};

const handleViewDetails = async (order: any) => {
  try {
    const fullOrder = await orderService.getById(order.id);
    selectedOrder.value = fullOrder;
    isDetailsOpen.value = true;
  } catch (error) {
    // Erro tratado pelo interceptor
  }
};

const isCancelModalOpen = ref(false);
const orderToCancel = ref<number | null>(null);

const handleCancel = (id: number) => {
  orderToCancel.value = id;
  isCancelModalOpen.value = true;
};

const confirmCancel = async () => {
  if (orderToCancel.value) {
    try {
      await orderService.updateStatus(orderToCancel.value, 'Cancelled', 'Cancelado pelo usuário');
      toast.success('Pedido cancelado com sucesso');
      fetchOrders();
    } catch (error) {
      // Erro tratado pelo interceptor
    } finally {
      isCancelModalOpen.value = false;
      orderToCancel.value = null;
    }
  }
};

const handleAddItem = () => {
  formData.value.items.push({ productId: '', quantity: 1, unitPrice: 0 });
};

const handleRemoveItem = (index: number) => {
  formData.value.items.splice(index, 1);
};

const handleItemChange = (index: number, field: string, value: any) => {
  const item = formData.value.items[index];
  if (field === 'productId') {
    const product = products.value.find(p => p.id === parseInt(value));
    item.productId = parseInt(value);
    item.unitPrice = product?.price || 0;
  } else {
    (item as any)[field] = value;
  }
};

const handleSubmit = async () => {
  if (formData.value.items.length === 0) return;
  if (formData.value.items.some(item => !item.productId)) return;

  try {
    if (editingId.value) {
      const order = orders.value.find(o => o.id === editingId.value);
      await orderService.update(editingId.value, {
        ...formData.value,
        status: order?.status || 'Pending'
      });
      toast.success('Pedido atualizado com sucesso');
    } else {
      await orderService.create(formData.value);
      toast.success('Pedido criado com sucesso');
    }
    isModalOpen.value = false;
    handleFetchOrders();
  } catch (error) {
    // Erro tratado pelo interceptor
  }
};

const clearFilters = () => {
  searchTerm.value = '';
  status.value = '';
  startDate.value = '';
  endDate.value = '';
};

const getStatusClass = (status: string) => {
  switch (status?.toLowerCase()) {
    case 'pending': return 'bg-amber-50 text-amber-600';
    case 'completed': return 'bg-emerald-50 text-emerald-600';
    case 'cancelled': return 'bg-rose-50 text-rose-600';
    case 'shipped': return 'bg-indigo-50 text-indigo-600';
    default: return 'bg-gray-50 text-gray-600';
  }
};

watch([page, pageSize, status, startDate, endDate], handleFetchOrders);
watch(searchTerm, () => {
  page.value = 1;
  handleFetchOrders();
});

onMounted(() => {
  handleFetchOrders();
  fetchProductsList();
});

</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.6s ease-out;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
