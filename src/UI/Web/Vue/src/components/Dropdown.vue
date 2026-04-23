<template>
  <div ref="dropdownRef" class="relative" :class="className">
    <button
      type="button"
      @click="toggle"
      class="w-full pl-8 pr-12 border-none outline-none font-black text-xs uppercase tracking-widest flex justify-between items-center transition-all active:scale-[0.98]"
      :class="[paddingClass, bgClass, textClass, roundedClass, shadowClass, hoverClass]"
    >
      <span class="truncate">{{ labelOverride ?? (selectedOption ? selectedOption.label : placeholder) }}</span>
      <svg 
        class="w-5 h-5 transition-transform duration-300" 
        :class="[{ 'rotate-180': isOpen }, iconColorClass]"
        fill="none" 
        stroke="currentColor" 
        viewBox="0 0 24 24"
      >
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M19 9l-7 7-7-7" />
      </svg>
    </button>

    <transition name="dropdown">
      <div 
        v-if="isOpen" 
        class="absolute z-50 w-full bg-white border border-gray-50 rounded-[2rem] shadow-2xl shadow-gray-200/50 py-3 overflow-hidden"
        :class="[direction === 'up' ? 'bottom-full mb-3' : 'mt-3']"
      >
        <div class="max-h-60 overflow-y-auto" @scroll="handleScroll">
          <button
            v-for="option in options"
            :key="option.value"
            type="button"
            @click="select(option)"
            class="w-full text-left px-8 py-4 text-[10px] font-black uppercase tracking-widest transition-colors"
            :class="[
              modelValue === option.value 
              ? 'bg-primary-50 text-primary-600' 
              : 'text-gray-500 hover:bg-gray-50'
            ]"
          >
            {{ option.label }}
          </button>
          
          <!-- Loading Indicator -->
          <div v-if="loading" class="px-8 py-4 flex justify-center">
            <div class="w-4 h-4 border-2 border-primary-600 border-t-transparent rounded-full animate-spin"></div>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';

interface Option {
  label: string;
  value: any;
}

const props = defineProps<{
  options: Option[];
  modelValue: any;
  placeholder?: string;
  className?: string;
  variant?: 'filter' | 'form' | 'primary';
  labelOverride?: string | null;
  direction?: 'up' | 'down';
  loading?: boolean;
}>();

const emit = defineEmits(['update:modelValue', 'change', 'loadMore']);

const isOpen = ref(false);
const dropdownRef = ref<HTMLElement | null>(null);

const selectedOption = computed(() => props.options.find(opt => opt.value === props.modelValue));

const paddingClass = computed(() => (props.variant === 'filter' || props.variant === 'primary') ? 'py-4' : 'py-3');
const roundedClass = computed(() => props.variant === 'filter' ? 'rounded-3xl' : 'rounded-2xl');
const bgClass = computed(() => props.variant === 'primary' ? 'bg-primary-600' : (props.variant === 'filter' ? 'bg-white' : 'bg-gray-50'));
const textClass = computed(() => props.variant === 'primary' ? 'text-white' : 'text-gray-900');
const hoverClass = computed(() => props.variant === 'primary' ? 'hover:bg-primary-700' : 'hover:bg-gray-50');
const shadowClass = computed(() => props.variant === 'primary' ? 'shadow-xl shadow-primary-200' : (props.variant === 'filter' ? 'shadow-xl shadow-gray-100/50' : ''));
const iconColorClass = computed(() => props.variant === 'primary' ? 'text-white/80' : 'text-gray-300');

const toggle = () => isOpen.value = !isOpen.value;
const handleScroll = (e: Event) => {
  const target = e.target as HTMLElement;
  if (target.scrollTop + target.clientHeight >= target.scrollHeight - 10) {
    emit('loadMore');
  }
};

const select = (option: Option) => {
  emit('update:modelValue', option.value);
  emit('change', option.value);
  isOpen.value = false;
};

const handleClickOutside = (event: MouseEvent) => {
  if (dropdownRef.value && !dropdownRef.value.contains(event.target as Node)) {
    isOpen.value = false;
  }
};

onMounted(() => document.addEventListener('mousedown', handleClickOutside));
onUnmounted(() => document.removeEventListener('mousedown', handleClickOutside));
</script>

<style scoped>
.dropdown-enter-active, .dropdown-leave-active {
  transition: all 0.2s ease-out;
}
.dropdown-enter-from, .dropdown-leave-to {
  opacity: 0;
  transform: scale(0.95);
}
</style>
