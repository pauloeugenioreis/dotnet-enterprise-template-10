<template>
  <transition name="modal">
    <div v-if="isOpen" class="fixed inset-0 z-[60] flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm">
      <div class="bg-white rounded-[2.5rem] shadow-2xl w-full max-w-sm overflow-hidden p-10 animate-scale-up">
        <div class="flex flex-col items-center text-center">
          <!-- Icon -->
          <div class="w-16 h-16 bg-red-50 rounded-2xl flex items-center justify-center mb-6">
            <svg class="w-8 h-8 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
          </div>
          
          <!-- Title -->
          <h3 class="text-xl font-black text-gray-900 mb-8 tracking-tight">{{ message || 'Deseja excluir este produto?' }}</h3>
          
          <!-- Actions -->
          <div class="flex gap-4 w-full">
            <button 
              @click="$emit('cancel')"
              class="flex-1 px-6 py-4 rounded-2xl border border-gray-100 text-gray-400 font-black text-xs uppercase tracking-widest hover:bg-gray-50 transition-all active:scale-95"
            >
              Cancelar
            </button>
            <button 
              @click="$emit('confirm')"
              class="flex-1 px-6 py-4 rounded-2xl bg-red-600 text-white font-black text-xs uppercase tracking-widest shadow-lg shadow-red-600/20 hover:bg-red-700 transition-all active:scale-95"
            >
              Confirmar
            </button>
          </div>
        </div>
      </div>
    </div>
  </transition>
</template>

<script setup lang="ts">
defineProps<{
  isOpen: boolean;
  title?: string;
  message?: string;
}>();

defineEmits(['confirm', 'cancel']);
</script>

<style scoped>
.modal-enter-active, .modal-leave-active {
  transition: opacity 0.3s ease;
}
.modal-enter-from, .modal-leave-to {
  opacity: 0;
}

.animate-scale-up {
  animation: scaleUp 0.4s cubic-bezier(0.34, 1.56, 0.64, 1);
}

@keyframes scaleUp {
  from { transform: scale(0.9); opacity: 0; }
  to { transform: scale(1); opacity: 1; }
}
</style>
