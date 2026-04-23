<template src="./Audit.html"></template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import axios from 'axios';
import Dropdown from '../../components/Dropdown.vue';
import Pagination from '../../components/Pagination.vue';

const logs = ref<any[]>([]);
const loading = ref(true);
const apiBase = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7196';

// Filters
const entityType = ref('');
const eventType = ref('');
const userId = ref('');

// Pagination
const page = ref(1);
const pageSize = ref(10);
const totalItems = ref(0);
const totalPages = ref(0);

const entityOptions = [
  { label: 'Todos', value: '' },
  { label: 'Pedidos', value: 'Order' },
  { label: 'Produtos', value: 'Product' }
];

const loadLogs = async () => {
  loading.value = true;
  try {
    let url = `${apiBase}/api/v1/audit?page=${page.value}&pageSize=${pageSize.value}`;
    if (entityType.value) url = `${apiBase}/api/v1/audit/${entityType.value}?page=${page.value}&pageSize=${pageSize.value}`;
    
    const { data } = await axios.get(url);
    logs.value = data.items;
    totalItems.value = data.totalCount;
    totalPages.value = data.totalPages;
  } catch (error) {
    console.error('Erro ao buscar logs');
  } finally {
    loading.value = false;
  }
};

onMounted(loadLogs);

const onFilter = () => {
  page.value = 1;
  loadLogs();
};

const clearFilters = () => {
  entityType.value = '';
  eventType.value = '';
  userId.value = '';
  page.value = 1;
  loadLogs();
};

const onPageChange = (newPage: number) => {
  page.value = newPage;
  loadLogs();
};
</script>
