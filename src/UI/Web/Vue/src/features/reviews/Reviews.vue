<template src="./Reviews.html"></template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { customerReviewService } from '../../api/services/CustomerReviewService';
import ConfirmModal from '../../components/ConfirmModal.vue';
import { useToast } from 'vue-toastification';

const toast = useToast();

const reviews = ref<any[]>([]);
const loading = ref(true);
const totalCount = ref(0);
const page = ref(1);
const pageSize = 8;

const productName = ref('');
const minRating = ref<number | undefined>(undefined);
const isApproved = ref<boolean | undefined>(undefined);

const loadReviews = async () => {
  loading.value = true;
  try {
    const data = await customerReviewService.list(page.value, pageSize, {
      productName: productName.value,
      minRating: minRating.value,
      isApproved: isApproved.value
    });
    reviews.value = data.items;
    totalCount.value = data.totalCount;
  } finally {
    loading.value = false;
  }
};

onMounted(loadReviews);

watch([page, isApproved, minRating], loadReviews);

const handleApprove = async (id: string, approve: boolean) => {
  await customerReviewService.approve(id, approve);
  toast.success(approve ? 'Avaliação aprovada' : 'Avaliação rejeitada');
  await loadReviews();
};

const isDeleteModalOpen = ref(false);
const reviewToDelete = ref<string | null>(null);

const handleDelete = (id: string) => {
  reviewToDelete.value = id;
  isDeleteModalOpen.value = true;
};

const confirmDelete = async () => {
  if (reviewToDelete.value) {
    try {
      await customerReviewService.delete(reviewToDelete.value);
      toast.success('Avaliação excluída com sucesso');
      await loadReviews();
    } catch (error) {
      // Erro tratado pelo interceptor
    } finally {
      isDeleteModalOpen.value = false;
      reviewToDelete.value = null;
    }
  }
};
</script>
