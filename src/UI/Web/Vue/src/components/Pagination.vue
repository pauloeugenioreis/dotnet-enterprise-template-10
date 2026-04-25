<template>
  <div class="bg-gray-50 px-10 py-6 flex justify-between items-center border-t border-gray-100">
    <div class="flex flex-col sm:flex-row sm:items-center gap-4 sm:gap-10">
      <p class="text-sm text-gray-500 font-bold">
        Página <span class="text-gray-900">{{ currentPage }}</span> de <span class="text-gray-900">{{ totalPages || 1 }}</span>
      </p>
      
      <div class="flex items-center gap-3">
        <span class="text-[10px] font-black uppercase tracking-widest text-gray-400">Itens por página:</span>
        <select 
          :value="pageSize"
          @change="$emit('update:pageSize', Number(($event.target as HTMLSelectElement).value))"
          class="bg-white border border-gray-200 rounded-xl px-4 py-2 text-xs font-black text-gray-900 focus:outline-none focus:ring-2 focus:ring-primary-600 transition-all cursor-pointer shadow-sm"
        >
          <option v-for="size in [5, 10, 20, 50, 100]" :key="size" :value="size">{{ size }}</option>
        </select>
      </div>
    </div>

    <div class="flex gap-3">
      <button
        @click="$emit('update:currentPage', Math.max(1, currentPage - 1))"
        :disabled="currentPage === 1"
        class="px-6 py-3 bg-white border border-gray-200 rounded-xl font-black text-xs uppercase tracking-widest hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-all shadow-sm active:scale-95"
      >
        Anterior
      </button>
      <button
        @click="$emit('update:currentPage', Math.min(totalPages, currentPage + 1))"
        :disabled="currentPage === totalPages || totalPages === 0"
        class="px-6 py-3 bg-primary-600 text-white rounded-xl font-black text-xs uppercase tracking-widest hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed transition-all shadow-lg shadow-primary-600/20 active:scale-95"
      >
        Próxima
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  currentPage: number;
  totalPages: number;
  pageSize: number;
}>();

defineEmits(['update:currentPage', 'update:pageSize']);
</script>
