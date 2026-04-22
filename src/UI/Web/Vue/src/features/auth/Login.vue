<template src="./Login.html"></template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../../store/auth';
import axios from 'axios';

const email = ref('');
const password = ref('');
const loading = ref(false);
const router = useRouter();
const authStore = useAuthStore();

const handleLogin = async () => {
  loading.value = true;
  try {
    const { data } = await axios.post('https://localhost:7196/api/auth/login', {
      email: email.value,
      password: password.value
    });
    authStore.setAuth(data, data.token);
    router.push('/dashboard');
  } catch (error) {
    alert('Erro ao entrar');
  } finally {
    loading.value = false;
  }
};
</script>
