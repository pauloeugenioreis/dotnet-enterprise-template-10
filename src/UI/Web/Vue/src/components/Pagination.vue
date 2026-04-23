<script setup lang="ts">
defineProps<{
  page: number;
  totalPages: number;
  totalItems: number;
}>();

const emit = defineEmits(['update:page']);

const onPageChange = (newPage: number) => {
  emit('update:page', newPage);
};
</script>

<template>
  <div class="flex items-center justify-between mt-10 px-8 py-6 bg-white rounded-[2.5rem] shadow-xl shadow-gray-100/50 border border-gray-50">
    <div class="flex gap-2">
      <button 
        @click="onPageChange(page - 1)"
        :disabled="page === 1"
        class="w-12 h-12 flex items-center justify-center rounded-2xl bg-gray-50 text-gray-400 hover:bg-primary-50 hover:text-primary-600 disabled:opacity-30 disabled:hover:bg-gray-50 disabled:hover:text-gray-400 transition-all active:scale-90"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="3" d="M15 19l-7-7 7-7" /></svg>
      </button>
      <div class="flex items-center px-6 bg-gray-50 rounded-2xl">
        <span class="text-xs font-black text-gray-900 uppercase tracking-widest">Página {{ page }} de {{ totalPages }}</span>
      </div>
      <button 
        @click="onPageChange(page + 1)"
        :disabled="page === totalPages"
        class="w-12 h-12 flex items-center justify-center rounded-2xl bg-gray-50 text-gray-400 hover:bg-primary-50 hover:text-primary-600 disabled:opacity-30 disabled:hover:bg-gray-50 disabled:hover:text-gray-400 transition-all active:scale-90"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="3" d="M9 5l7 7-7 7" /></svg>
      </button>
    </div>
    <div class="text-[10px] font-black text-gray-300 uppercase tracking-[0.2em]">Total de {{ totalItems }} registros</div>
  </div>
</template>
