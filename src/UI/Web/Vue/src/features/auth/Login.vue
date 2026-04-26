<template src="./Login.html"></template>

<script setup lang="ts">
import { ref } from 'vue';
import { useAuth } from '../../composables/api/useAuth';

const email = ref('admin@projecttemplate.com');
const password = ref('Admin@2026!Secure');
const { loading, login } = useAuth();

const handleLogin = async () => {
  try {
    await login({
      email: email.value,
      password: password.value
    });
  } catch (error) {
    alert('Erro ao entrar');
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
