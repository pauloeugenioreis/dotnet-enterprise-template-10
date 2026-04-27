<template>
  <Drawer 
    :isOpen="isOpen" 
    @onClose="$emit('onClose')" 
    :title="'Pedido #' + order?.orderNumber"
    :subtitle="order ? 'Realizado em ' + formatDate(order.createdAt) : ''"
  >
    <template v-if="order">
      <div class="space-y-10 print-area">
        <!-- Header para Impressão (Apenas número) -->
        <div class="hidden print:flex justify-end border-b-2 border-gray-900 pb-4 mb-8">
          <div class="text-right">
            <p class="text-[10px] font-black uppercase tracking-widest text-gray-400">Pedido Número</p>
            <p class="text-2xl font-black text-gray-900">#{{ order.orderNumber }}</p>
          </div>
        </div>
        <!-- Status & Info -->
        <div class="grid grid-cols-2 gap-8">
          <div>
            <p class="text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Status do Pedido</p>
            <span :class="['px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest', getStatusClass(order.status)]">
              {{ order.status }}
            </span>
          </div>
          <div>
            <p class="text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Cliente</p>
            <p class="font-bold text-gray-900">{{ order.customerName }}</p>
            <p class="text-xs text-gray-500">{{ order.customerEmail }}</p>
          </div>
        </div>

        <!-- Items Table -->
        <div>
          <p class="text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-4">Itens do Pedido</p>
          <div class="bg-gray-50 rounded-3xl overflow-hidden border border-gray-100">
            <table class="w-full text-left">
              <thead class="bg-gray-100/50 text-[10px] uppercase text-gray-400">
                <tr>
                  <th class="px-6 py-4">Produto</th>
                  <th class="px-6 py-4 text-center">Qtd</th>
                  <th class="px-6 py-4 text-right">Preço</th>
                  <th class="px-6 py-4 text-right">Total</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-100">
                <tr v-for="item in order.items" :key="item.productId">
                  <td class="px-6 py-4">
                    <p class="text-sm font-bold text-gray-800">{{ item.productName }}</p>
                    <p class="text-[10px] text-gray-400">ID: {{ item.productId }}</p>
                  </td>
                  <td class="px-6 py-4 text-center text-sm font-medium text-gray-600">{{ item.quantity }}</td>
                  <td class="px-6 py-4 text-right text-sm text-gray-600">R$ {{ item.unitPrice.toLocaleString('pt-BR') }}</td>
                  <td class="px-6 py-4 text-right text-sm font-bold text-gray-900">R$ {{ item.subtotal.toLocaleString('pt-BR') }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Shipping Address -->
        <div>
          <p class="text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Endereço de Entrega</p>
          <div class="p-5 bg-primary-50/30 rounded-2xl border border-primary-100/50 flex gap-4">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-primary-600 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
            <p class="text-sm text-gray-700 leading-relaxed">{{ order.shippingAddress }}</p>
          </div>
        </div>

        <!-- Totals Summary -->
        <div class="space-y-3 pt-6 border-t border-gray-100">
          <div class="flex justify-between text-sm">
            <span class="text-gray-500">Subtotal</span>
            <span class="font-medium text-gray-900">R$ {{ order.subtotal.toLocaleString('pt-BR') }}</span>
          </div>
          <div class="flex justify-between text-sm">
            <span class="text-gray-500">Frete</span>
            <span class="font-medium text-gray-900">R$ {{ order.shippingCost.toLocaleString('pt-BR') }}</span>
          </div>
          <div class="flex justify-between text-sm">
            <span class="text-gray-500">Impostos</span>
            <span class="font-medium text-gray-900">R$ {{ order.tax.toLocaleString('pt-BR') }}</span>
          </div>
          <div class="pt-4 border-t border-gray-200 flex justify-between items-end">
            <span class="text-lg font-bold text-gray-900">Total</span>
            <span class="text-3xl font-black text-primary-600">R$ {{ order.total.toLocaleString('pt-BR') }}</span>
          </div>
        </div>
      </div>
    </template>

    <template #footer>
      <div class="flex gap-4 no-print">
        <button @click="print" class="flex-1 bg-gradient-to-br from-primary-50/50 to-white border border-primary-100 py-4 rounded-2xl text-[11px] font-bold text-primary-700 uppercase tracking-widest hover:bg-white hover:border-primary-200 hover:shadow-lg hover:shadow-primary-100/50 transition-all active:scale-95 flex items-center justify-center gap-3">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-primary-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" />
          </svg>
          Imprimir Pedido
        </button>
        <div class="flex-1">
          <Dropdown 
            v-model="orderStatus"
            :options="statusOptions"
            variant="primary"
            labelOverride="Atualizar Status"
            direction="up"
            @change="handleStatusChange"
          />
        </div>
      </div>
    </template>
  </Drawer>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useToast } from 'vue-toastification';
import { orderService } from '../../../api/services/OrderService';
import Dropdown from '../../../components/Dropdown.vue';
import Drawer from '../../../components/Drawer.vue';

const props = defineProps<{
  isOpen: boolean;
  order: any;
}>();

const emit = defineEmits(['onClose', 'onStatusUpdated']);

const orderStatus = ref('');
const statusOptions = [
  { label: 'Pendente', value: 'Pending' },
  { label: 'Enviado', value: 'Shipped' },
  { label: 'Entregue', value: 'Delivered' },
  { label: 'Cancelado', value: 'Cancelled' }
];

watch(() => props.order, (newOrder) => {
  if (newOrder) orderStatus.value = newOrder.status;
});

const handleStatusChange = async (newStatus: string) => {
  if (!props.order) return;
  const toast = useToast();
  try {
    await orderService.updateStatus(props.order.id, newStatus, 'Atualizado via Dashboard Vue');
    toast.success('Status atualizado com sucesso');
    emit('onStatusUpdated');
  } catch (error) {
    // Erro tratado pelo interceptor
  }
};

const print = () => window.print();

const formatDate = (date: string) => {
  if (!date) return '';
  return new Date(date).toLocaleString('pt-BR');
};

const getStatusClass = (status: string) => {
  switch (status?.toLowerCase()) {
    case 'pending': return 'bg-amber-50 text-amber-600';
    case 'completed': return 'bg-emerald-50 text-emerald-600';
    case 'cancelled': return 'bg-rose-50 text-rose-600';
    case 'shipped': return 'bg-indigo-50 text-indigo-600';
    default: return 'bg-gray-50 text-gray-600';
  }
};
</script>

<style scoped>
@media print {
  .no-print { display: none; }
}
</style>
