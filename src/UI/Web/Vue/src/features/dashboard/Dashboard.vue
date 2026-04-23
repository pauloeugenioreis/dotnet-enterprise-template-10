<template src="./Dashboard.html"></template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '../../api/api';

const stats = [
  { label: 'Vendas Totais', value: 'R$ 124.500', icon: '💰', color: 'bg-emerald-50 text-emerald-600' },
  { label: 'Pedidos', value: '1.240', icon: '🛍️', color: 'bg-primary-50 text-primary-600' },
  { label: 'Clientes', value: '850', icon: '👥', color: 'bg-amber-50 text-amber-600' },
];

const latestOrders = ref<any[]>([]);
const loading = ref(true);

onMounted(async () => {
  try {
    const { data } = await api.get('/api/v1/order?page=1&pageSize=5');
    latestOrders.value = data.items;
  } catch (error) {
    console.error('Erro ao carregar dashboard', error);
  } finally {
    loading.value = false;
  }
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
