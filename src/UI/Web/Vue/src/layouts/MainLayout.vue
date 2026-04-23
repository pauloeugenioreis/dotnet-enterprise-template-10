<template>
  <div class="flex h-screen bg-gray-100">
    <!-- Sidebar -->
    <div class="w-64 bg-white shadow-lg relative flex flex-col">
      <div class="p-6 border-b flex items-center gap-3">
        <div class="w-10 h-10 bg-primary-600 rounded-lg flex items-center justify-center text-white font-bold text-xl">
          ET
        </div>
        <span class="font-bold text-xl text-gray-800">Enterprise</span>
      </div>
      
      <nav class="mt-6 px-4 space-y-2 flex-1">
        <router-link
          v-for="item in menuItems"
          :key="item.path"
          :to="item.path"
          class="flex items-center gap-3 px-4 py-3 rounded-lg transition-colors"
          :class="[
            route.path === item.path
              ? 'bg-primary-50 text-primary-600'
              : 'text-gray-600 hover:bg-gray-50'
          ]"
        >
          <span>{{ item.icon }}</span>
          <span class="font-medium">{{ item.name }}</span>
        </router-link>
      </nav>

      <div class="p-6 border-t bg-white">
        <div class="flex items-center gap-3 mb-4">
          <div class="w-10 h-10 bg-gray-200 rounded-full flex items-center justify-center text-gray-600 font-bold uppercase">
            {{ userInitial }}
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-semibold text-gray-800 truncate">
              {{ userName }}
            </p>
            <p class="text-xs text-gray-500 truncate">{{ userEmail }}</p>
          </div>
        </div>
        <button
          @click="handleLogout"
          class="w-full flex items-center justify-center gap-2 px-4 py-2 border border-red-200 text-red-600 rounded-lg hover:bg-red-50 transition-colors text-sm font-medium"
        >
          🚪 Sair
        </button>
      </div>
    </div>

    <!-- Main Content -->
    <div class="flex-1 overflow-auto p-8">
      <router-view></router-view>
    </div>
  </div>
</template>

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

const userName = computed(() => {
  if (!authStore.user) return 'Usuário';
  return `${authStore.user.firstName} ${authStore.user.lastName}`;
});

const userEmail = computed(() => authStore.user?.email || '');
const userInitial = computed(() => authStore.user?.firstName?.[0] || 'A');

const handleLogout = () => {
  authStore.logout();
  router.push('/login');
};
</script>
