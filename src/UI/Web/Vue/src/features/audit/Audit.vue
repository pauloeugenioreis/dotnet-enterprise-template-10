<template src="./Audit.html"></template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import axios from 'axios';

const logs = ref<any[]>([]);
const loading = ref(true);

onMounted(async () => {
  try {
    const apiBase = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7196';
    const { data } = await axios.get(`${apiBase}/api/v1/audit/Order?page=1&pageSize=10`);
    logs.value = data.items;
  } catch (error) {
    console.error('Erro ao buscar logs');
  } finally {
    loading.value = false;
  }
});
</script>
