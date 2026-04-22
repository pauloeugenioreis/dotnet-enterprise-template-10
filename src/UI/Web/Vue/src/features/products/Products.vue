<template src="./Products.html"></template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import axios from 'axios';

const products = ref<any[]>([]);
const loading = ref(true);

onMounted(async () => {
  try {
    const apiBase = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7196';
    const { data } = await axios.get(`${apiBase}/api/products?page=1&pageSize=10`);
    products.value = data.items;
  } catch (error) {
    console.error('Erro ao buscar produtos');
  } finally {
    loading.value = false;
  }
});
</script>
