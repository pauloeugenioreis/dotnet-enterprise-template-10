<template src="./Login.html"></template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../../store/auth';
import axios from 'axios';

const email = ref('admin@projecttemplate.com');
const password = ref('Admin@2026!Secure');
const loading = ref(false);
const router = useRouter();
const authStore = useAuthStore();

const handleLogin = async () => {
  loading.value = true;
  try {
    const apiBase = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7196';
    const { data } = await axios.post(`${apiBase}/api/auth/login`, {
      email: email.value,
      password: password.value
    });
    authStore.setAuth(data.user, data.accessToken);
    router.push('/dashboard');
  } catch (error) {
    alert('Erro ao entrar');
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
@keyframes slideInFromBottom {
  from { transform: translateY(1rem); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

.animate-in {
  animation: slideInFromBottom 0.7s ease-out forwards;
}
</style>
