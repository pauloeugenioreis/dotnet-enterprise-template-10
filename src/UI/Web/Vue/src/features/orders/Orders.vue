<template src="./Orders.html"></template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import axios from 'axios';
import Dropdown from '../../components/Dropdown.vue';
import Pagination from '../../components/Pagination.vue';

const orders = ref<any[]>([]);
const loading = ref(true);
const apiBase = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7196';

// Filters
const searchTerm = ref('');
const status = ref('');
const fromDate = ref('');
const toDate = ref('');

// Pagination
const page = ref(1);
const pageSize = ref(10);
const totalItems = ref(0);
const totalPages = ref(0);

const statusOptions = [
  { label: 'Todos os Status', value: '' },
  { label: 'Pendente', value: 'Pending' },
  { label: 'Enviado', value: 'Shipped' },
  { label: 'Entregue', value: 'Delivered' },
  { label: 'Cancelado', value: 'Cancelled' }
];

const loadOrders = async () => {
  loading.value = true;
  try {
    let url = `${apiBase}/api/v1/Order?page=${page.value}&pageSize=${pageSize.value}`;
    if (searchTerm.value) url += `&searchTerm=${encodeURIComponent(searchTerm.value)}`;
    if (status.value) url += `&status=${status.value}`;
    if (fromDate.value) url += `&from=${new Date(fromDate.value).toISOString()}`;
    if (toDate.value) url += `&toDate=${new Date(toDate.value).toISOString()}`;
    
    const { data } = await axios.get(url);
    orders.value = data.items;
    totalItems.value = data.totalCount;
    totalPages.value = data.totalPages;
  } catch (error) {
    console.error('Erro ao buscar pedidos');
  } finally {
    loading.value = false;
  }
};

onMounted(loadOrders);

const onFilter = () => {
  page.value = 1;
  loadOrders();
};

const clearFilters = () => {
  searchTerm.value = '';
  status.value = '';
  fromDate.value = '';
  toDate.value = '';
  page.value = 1;
  loadOrders();
};

const onPageChange = (newPage: number) => {
  page.value = newPage;
  loadOrders();
};

const getStatusClass = (status: string) => {
  switch (status.toLowerCase()) {
    case 'delivered': case 'entregue': return 'bg-emerald-50 text-emerald-600';
    case 'pending': case 'pendente': return 'bg-amber-50 text-amber-600';
    case 'shipped': case 'enviado': return 'bg-blue-50 text-blue-600';
    case 'cancelled': case 'cancelado': return 'bg-red-50 text-red-600';
    default: return 'bg-gray-50 text-gray-600';
  }
};
</script>
