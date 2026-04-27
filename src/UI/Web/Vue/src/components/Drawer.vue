<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-end">
    <!-- Backdrop -->
    <div 
      class="absolute inset-0 bg-gray-900/40 backdrop-blur-sm transition-opacity no-print" 
      @click="$emit('onClose')"
    ></div>

    <!-- Drawer Content -->
    <div class="relative h-full w-full max-w-2xl bg-white shadow-2xl flex flex-col animate-slide-in">
      <!-- Header -->
      <div class="p-8 border-b border-gray-100 flex justify-between items-center bg-gray-50/50 no-print">
        <div>
          <h2 class="text-2xl font-black text-gray-900 tracking-tight">{{ title }}</h2>
          <p v-if="subtitle" class="text-gray-500 text-sm mt-1">{{ subtitle }}</p>
        </div>
        <button 
          @click="$emit('onClose')" 
          class="p-2 hover:bg-gray-200 rounded-xl transition-all no-print"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <!-- Body -->
      <div class="flex-1 overflow-y-auto p-8">
        <slot></slot>
      </div>

      <!-- Footer -->
      <div v-if="$slots.footer" class="p-8 border-t border-gray-100 bg-gray-50/30 no-print">
        <slot name="footer"></slot>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  isOpen: boolean;
  title: string;
  subtitle?: string;
}>();

defineEmits(['onClose']);
</script>

<style scoped>
@keyframes slideIn {
  from { transform: translateX(100%); }
  to { transform: translateX(0); }
}
.animate-slide-in {
  animation: slideIn 0.3s ease-out forwards;
}
</style>
