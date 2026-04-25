<template src="./Orders.html"></template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue';
import { orderService } from '../../api/services/OrderService';
import { productService } from '../../api/services/ProductService';
import Dropdown from '../../components/Dropdown.vue';
import Pagination from '../../components/Pagination.vue';
import Modal from '../../components/Modal.vue';
import Drawer from '../../components/Drawer.vue';
import OrderDetailsModal from './components/OrderDetailsModal.vue';

const orders = ref<any[]>([]);
const loading = ref(true);
const page = ref(1);
const pageSize = ref(10);
const totalPages = ref(1);
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

const products = ref<any[]>([]);
const productPage = ref(1);
const productLoading = ref(false);
const hasMoreProducts = ref(true);

const fetchProducts = async (isLoadMore = false) => {
  if (productLoading.value || (!hasMoreProducts.value && isLoadMore)) return;
  
  productLoading.value = true;
  try {
    const data = await productService.getAll({ page: productPage.value, pageSize: 20 });
    const newItems = Array.isArray(data) ? data : (data?.items || []);
    
    if (isLoadMore) {
      products.value = [...products.value, ...newItems];
    } else {
      products.value = newItems;
    }
    
    hasMoreProducts.value = newItems.length === 20;
    if (hasMoreProducts.value) productPage.value++;
  } catch (error) {
    console.error('Erro ao carregar produtos', error);
  } finally {
    productLoading.value = false;
  }
};

const handleLoadMoreProducts = () => {
  if (hasMoreProducts.value) {
    fetchProducts(true);
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

const fetchOrders = async () => {
  loading.value = true;
  try {
    const data = await orderService.getAll({
      page: page.value,
      pageSize: pageSize.value,
      searchTerm: searchTerm.value,
      status: status.value,
      startDate: startDate.value,
      endDate: endDate.value
    });
    orders.value = data?.items || [];
    totalPages.value = data.totalPages;
  } catch (error) {
    console.error('Erro ao carregar pedidos', error);
  } finally {
    loading.value = false;
  }
};

const handleExport = () => orderService.exportToExcel();

const handleOpenNew = () => {
  editingId.value = null;
  formData.value = { customerName: '', customerEmail: '', shippingAddress: '', items: [] };
  isModalOpen.value = true;
};

const handleEdit = (order: any) => {
  editingId.value = order.id;
  formData.value = {
    customerName: order.customerName || '',
    customerEmail: order.customerEmail || '',
    shippingAddress: order.shippingAddress || '',
    items: order.items ? order.items.map((item: any) => ({
      productId: item.productId,
      quantity: item.quantity,
      unitPrice: item.unitPrice
    })) : []
  };
  isModalOpen.value = true;
};

const handleViewDetails = (order: any) => {
  selectedOrder.value = order;
  isDetailsOpen.value = true;
};

const handleCancel = async (id: number) => {
  if (!confirm('Tem certeza que deseja cancelar este pedido?')) return;
  try {
    await orderService.updateStatus(id, 'Cancelled', 'Cancelado pelo usuário');
    fetchOrders();
  } catch (error) {
    alert('Erro ao cancelar pedido');
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
  try {
    if (editingId.value) {
      await orderService.update(editingId.value, formData.value);
    } else {
      await orderService.create(formData.value);
    }
    isModalOpen.value = false;
    fetchOrders();
  } catch (error) {
    alert('Erro ao salvar pedido. Verifique se todos os campos estão preenchidos.');
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

watch([page, pageSize, status, startDate, endDate], fetchOrders);
watch(searchTerm, () => {
  page.value = 1;
  fetchOrders();
});

onMounted(() => {
  fetchOrders();
  fetchProducts();
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
