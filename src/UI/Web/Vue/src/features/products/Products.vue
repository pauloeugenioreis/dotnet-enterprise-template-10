<template src="./Products.html"></template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import axios from 'axios';
import Dropdown from '../../components/Dropdown.vue';
import Modal from '../../components/Modal.vue';
import Pagination from '../../components/Pagination.vue';

const products = ref<any[]>([]);
const loading = ref(true);
const apiBase = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7196';

// Filters
const searchTerm = ref('');
const isActive = ref<boolean | undefined>(undefined);

// Pagination
const page = ref(1);
const pageSize = ref(10);
const totalItems = ref(0);
const totalPages = ref(0);

// Modal
const showModal = ref(false);
const modalTitle = ref('Novo Produto');
const editingProduct = ref<any>(null);
const formData = ref({
  name: '',
  category: '',
  price: 0,
  stock: 0,
  isActive: true
});

const loadProducts = async () => {
  loading.value = true;
  try {
    let url = `${apiBase}/api/v1/Product?page=${page.value}&pageSize=${pageSize.value}`;
    if (searchTerm.value) url += `&searchTerm=${encodeURIComponent(searchTerm.value)}`;
    if (isActive.value !== undefined) url += `&isActive=${isActive.value}`;
    
    const { data } = await axios.get(url);
    products.value = data.items;
    totalItems.value = data.totalCount;
    totalPages.value = data.totalPages;
  } catch (error) {
    console.error('Erro ao buscar produtos');
  } finally {
    loading.value = false;
  }
};

onMounted(loadProducts);

const onSearch = () => {
  page.value = 1;
  loadProducts();
};

const onStatusFilter = (val: boolean | undefined) => {
  isActive.value = val;
  page.value = 1;
  loadProducts();
};

const onPageChange = (newPage: number) => {
  page.value = newPage;
  loadProducts();
};

const openNew = () => {
  editingProduct.value = null;
  modalTitle.value = 'Novo Produto';
  formData.value = { name: '', category: '', price: 0, stock: 0, isActive: true };
  showModal.value = true;
};

const openEdit = (product: any) => {
  editingProduct.value = product;
  modalTitle.value = 'Editar Produto';
  formData.value = { ...product };
  showModal.value = true;
};

const save = async () => {
  try {
    if (editingProduct.value) {
      await axios.put(`${apiBase}/api/v1/Product/${editingProduct.value.id}`, formData.value);
    } else {
      await axios.post(`${apiBase}/api/v1/Product`, formData.value);
    }
    showModal.value = false;
    loadProducts();
  } catch (error: any) {
    alert('Erro ao salvar produto: ' + (error.response?.data?.message || 'Erro desconhecido'));
  }
};

const deleteProduct = async (id: string) => {
  if (confirm('Deseja excluir este produto?')) {
    try {
      await axios.delete(`${apiBase}/api/v1/Product/${id}`);
      loadProducts();
    } catch (error) {
      alert('Erro ao excluir produto');
    }
  }
};
</script>
