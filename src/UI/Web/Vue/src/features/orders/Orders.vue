<template src="./Orders.html"></template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { orderService } from '../../api/services/OrderService';
import Dropdown from '../../components/Dropdown.vue';
import Pagination from '../../components/Pagination.vue';
import Modal from '../../components/Modal.vue';
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
    orders.value = data.items;
    totalPages.value = data.totalPages;
  } catch (error) {
    console.error('Erro ao carregar pedidos', error);
  } finally {
    loading.value = false;
  }
};

const handleExport = () => orderService.exportToExcel();

const handleOpenNew = () => {
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

onMounted(fetchOrders);
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
