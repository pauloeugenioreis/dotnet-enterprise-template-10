<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';

interface Option {
  label: string;
  value: any;
}

const props = defineProps<{
  options: Option[];
  modelValue: any;
  placeholder?: string;
  variant?: 'filter' | 'form';
  className?: string;
}>();

const emit = defineEmits(['update:modelValue']);

const isOpen = ref(false);
const dropdownRef = ref<HTMLElement | null>(null);

const toggle = () => {
  isOpen.value = !isOpen.value;
};

const select = (option: Option) => {
  emit('update:modelValue', option.value);
  isOpen.value = false;
};

const handleClickOutside = (event: MouseEvent) => {
  if (dropdownRef.value && !dropdownRef.value.contains(event.target as Node)) {
    isOpen.value = false;
  }
};

onMounted(() => document.addEventListener('mousedown', handleClickOutside));
onUnmounted(() => document.removeEventListener('mousedown', handleClickOutside));

const selectedOption = () => props.options.find(opt => opt.value === props.modelValue);

const paddingClass = props.variant === 'form' ? 'py-3' : 'py-5';
const roundedClass = props.variant === 'form' ? 'rounded-2xl' : 'rounded-3xl';
const bgClass = props.variant === 'form' ? 'bg-gray-50' : 'bg-white';
const shadowClass = props.variant === 'form' ? '' : 'shadow-xl shadow-gray-100/50';
</script>

<template>
  <div ref="dropdownRef" class="relative" :class="className">
    <button
      type="button"
      @click="toggle"
      class="w-full pl-8 pr-12 border-none outline-none font-black text-xs uppercase tracking-widest text-gray-900 flex justify-between items-center hover:bg-gray-50 transition-all active:scale-[0.98]"
      :class="[paddingClass, bgClass, roundedClass, shadowClass]"
    >
      <span class="truncate">{{ selectedOption() ? selectedOption()?.label : (placeholder || 'Selecionar...') }}</span>
      <svg 
        class="w-5 h-5 text-gray-300 transition-transform duration-300"
        :class="{ 'rotate-180': isOpen }"
        fill="none" 
        stroke="currentColor" 
        viewBox="0 0 24 24"
      >
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="3" d="M19 9l-7 7-7-7" />
      </svg>
    </button>

    <Transition
      enter-active-class="transition duration-200 ease-out"
      enter-from-class="transform scale-95 opacity-0"
      enter-to-class="transform scale-100 opacity-100"
      leave-active-class="transition duration-150 ease-in"
      leave-from-class="transform scale-100 opacity-100"
      leave-to-class="transform scale-95 opacity-0"
    >
      <div v-if="isOpen" class="absolute z-50 w-full mt-3 bg-white border border-gray-50 rounded-[2rem] shadow-2xl shadow-gray-200/50 py-3 overflow-hidden">
        <div class="max-h-60 overflow-y-auto">
          <button
            v-for="option in options"
            :key="option.value"
            type="button"
            @click="select(option)"
            class="w-full text-left px-8 py-4 text-[10px] font-black uppercase tracking-widest transition-colors"
            :class="modelValue === option.value ? 'bg-primary-50 text-primary-600' : 'text-gray-500 hover:bg-gray-50'"
          >
            {{ option.label }}
          </button>
        </div>
      </div>
    </Transition>
  </div>
</template>
