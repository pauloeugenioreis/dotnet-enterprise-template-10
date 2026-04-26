<template src="./Audit.html"></template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { auditService } from '../../api/services/AuditService';
import Pagination from '../../components/Pagination.vue';
import Dropdown from '../../components/Dropdown.vue';

const auditLogs = ref<any[]>([]);
const loading = ref(true);
const page = ref(1);
const pageSize = ref(10);
const totalPages = ref(1);

const entityType = ref('');
const eventType = ref('');
const userId = ref('');
const startDate = ref('');
const endDate = ref('');

const selectedLog = ref<any>(null);

const fetchAuditLogs = async () => {
  loading.value = true;
  try {
    const data = await auditService.getAuditLogs(page.value, pageSize.value, {
      page: page.value,
      pageSize: pageSize.value,
      entityType: entityType.value,
      eventType: eventType.value,
      userId: userId.value,
      startDate: startDate.value,
      endDate: endDate.value
    });
    auditLogs.value = data.items;
    totalPages.value = data.totalPages;
  } catch (error) {
    console.error('Erro ao carregar logs de auditoria', error);
  } finally {
    loading.value = false;
  }
};

const formatDate = (date: string) => {
  if (!date) return '-';
  const d = new Date(date);
  return isNaN(d.getTime()) ? 'Data Inválida' : d.toLocaleString('pt-BR');
};

const viewDetails = (log: any) => {
  selectedLog.value = log;
};

const clearFilters = () => {
  entityType.value = '';
  eventType.value = '';
  userId.value = '';
  startDate.value = '';
  endDate.value = '';
};

watch([page, pageSize, entityType, startDate, endDate], fetchAuditLogs);
watch([eventType, userId], () => {
  page.value = 1;
  fetchAuditLogs();
});

onMounted(fetchAuditLogs);
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.6s ease-out;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
