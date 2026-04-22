<template src="./Orders.html"></template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import axios from 'axios';

const orders = ref<any[]>([]);
const loading = ref(true);

const getStatusColor = (status: string) => {
  switch (status.toLowerCase()) {
    case 'delivered': return 'bg-green-100 text-green-700';
    case 'pending': return 'bg-amber-100 text-amber-700';
    case 'cancelled': return 'bg-red-100 text-red-700';
    default: return 'bg-gray-100 text-gray-700';
  }
};

onMounted(async () => {
  try {
    const { data } = await axios.get('https://localhost:7196/api/orders?page=1&pageSize=10');
    orders.value = data.items;
  } catch (error) {
    console.error('Erro ao buscar pedidos');
  } finally {
    loading.value = false;
  }
});
</script>
