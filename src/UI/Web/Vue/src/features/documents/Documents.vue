<template src="./Documents.html"></template>

<script setup lang="ts">
import { ref } from 'vue';
import { documentService } from '../../api/services/DocumentService';

const fileName = ref('');
const uploading = ref(false);
const processing = ref(false);

const handleFileSelected = async (event: any) => {
  const file = event.target.files[0];
  if (file) {
    uploading.value = true;
    try {
      const res = await documentService.upload(file);
      fileName.value = res.fileName;
    } finally {
      uploading.value = false;
    }
  }
};

const handleDownload = async () => {
  if (!fileName.value) return;
  processing.value = true;
  try {
    await documentService.download(fileName.value);
  } finally {
    processing.value = false;
  }
};

const handleDelete = async () => {
  if (!fileName.value) return;
  processing.value = true;
  try {
    await documentService.delete(fileName.value);
    fileName.value = '';
  } finally {
    processing.value = false;
  }
};
</script>

<style scoped>
.animate-in {
  animation: fadeIn 0.7s ease-out;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
