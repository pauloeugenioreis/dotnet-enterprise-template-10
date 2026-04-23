<template>
  <Transition name="fade">
    <div v-if="loadingStore.isLoading" class="fixed inset-0 z-[9999] flex items-center justify-center bg-gray-900/20 backdrop-blur-[2px]">
      <div class="bg-white p-8 rounded-[2rem] shadow-2xl flex flex-col items-center gap-4 animate-scale-in">
        <div class="relative w-16 h-16">
          <div class="absolute inset-0 border-4 border-primary-100 rounded-full"></div>
          <div class="absolute inset-0 border-4 border-primary-600 rounded-full border-t-transparent animate-spin"></div>
        </div>
        <p class="text-[10px] font-black uppercase tracking-[0.3em] text-primary-600 animate-pulse">Carregando</p>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { useLoadingStore } from '../store/loading';
const loadingStore = useLoadingStore();
</script>

<style scoped>
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}

@keyframes scaleIn {
  from { opacity: 0; transform: scale(0.9); }
  to { opacity: 1; transform: scale(1); }
}
.animate-scale-in {
  animation: scaleIn 0.3s ease-out forwards;
}
</style>
