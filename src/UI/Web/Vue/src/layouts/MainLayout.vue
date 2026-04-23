<template src="./MainLayout.html"></template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from '../store/auth';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();

const menuItems = [
  { name: 'Dashboard', path: '/dashboard', icon: '📊' },
  { name: 'Produtos', path: '/products', icon: '📦' },
  { name: 'Pedidos', path: '/orders', icon: '🛍️' },
  { name: 'Auditoria', path: '/audit', icon: '🛡️' },
];

const isAdmin = computed(() => authStore.user?.email?.toLowerCase() === 'admin@projecttemplate.com');
const userName = computed(() => isAdmin.value ? 'System Administrator' : (authStore.user?.fullName || 'Usuário'));
const userEmail = computed(() => authStore.user?.email || 'admin@projecttemplate.com');
const userInitial = computed(() => isAdmin.value ? 'S' : (authStore.user?.fullName?.[0] || authStore.user?.email?.[0] || 'U').toUpperCase());

const handleLogout = () => {
  authStore.logout();
  router.push('/login');
};
</script>

<style scoped>
.router-link-active {
  /* Tailwind classes handle this, but keeping scoped for specific tweaks if needed */
}
</style>
