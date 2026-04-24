<template src="./Dashboard.html"></template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { orderService } from '../../api/services/OrderService';

const stats = [
  { label: 'Vendas Mensais', value: 'R$ 842.500,00', icon: '📈', color: 'bg-rose-50 text-rose-500' },
  { label: 'Novos Clientes', value: '+ 128', icon: '👤', color: 'bg-blue-50 text-blue-500' },
  { label: 'Status do Sistema', value: 'Operacional', icon: '✅', color: 'bg-emerald-50 text-emerald-500' },
];

const latestOrders = ref<any[]>([]);
const loading = ref(true);

onMounted(async () => {
  try {
    const data = await orderService.getAll({ page: 1, pageSize: 5 });
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
