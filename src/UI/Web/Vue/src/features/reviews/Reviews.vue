<template src="./Reviews.html"></template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { customerReviewService } from '../../api/services/CustomerReviewService';

const reviews = ref<any[]>([]);
const loading = ref(true);
const totalCount = ref(0);
const page = ref(1);
const pageSize = 8;

// Filters
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
  await loadReviews();
};

const handleDelete = async (id: string) => {
  if (confirm('Tem certeza que deseja excluir esta avaliação?')) {
    await customerReviewService.delete(id);
    await loadReviews();
  }
};
</script>
