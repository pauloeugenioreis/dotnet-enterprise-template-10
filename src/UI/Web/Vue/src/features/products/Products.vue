<template src="./Products.html"></template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { productService } from '../../api/services/ProductService';
import Pagination from '../../components/Pagination.vue';
import Modal from '../../components/Modal.vue';

const products = ref<any[]>([]);
const loading = ref(true);
const page = ref(1);
const pageSize = ref(10);
const totalPages = ref(1);
const searchTerm = ref('');
const isActiveFilter = ref<boolean | undefined>(undefined);

const statusFilters = [
  { label: 'Todos', value: undefined },
  { label: 'Ativos', value: true },
  { label: 'Inativos', value: false }
];

const isModalOpen = ref(false);
const editingId = ref<number | null>(null);
const formData = ref({ name: '', category: '', price: '0', stock: '0', isActive: true });

const fetchProducts = async () => {
  loading.value = true;
  try {
    const data = await productService.getAll({
      page: page.value,
      pageSize: pageSize.value,
      searchTerm: searchTerm.value,
      isActive: isActiveFilter.value
    });
    products.value = data.items;
    totalPages.value = data.totalPages;
  } catch (error) {
    console.error('Erro ao carregar produtos', error);
  } finally {
    loading.value = false;
  }
};

const handleExport = () => productService.exportToExcel();

const handleOpenNew = () => {
  editingId.value = null;
  formData.value = { name: '', category: '', price: '0', stock: '0', isActive: true };
  isModalOpen.value = true;
};

const handleEdit = (product: any) => {
  editingId.value = product.id;
  formData.value = { 
    name: product.name, 
    category: product.category, 
    price: product.price.toString(), 
    stock: product.stock.toString(), 
    isActive: product.isActive 
  };
  isModalOpen.value = true;
};

const handleDelete = async (id: number) => {
  if (!confirm('Excluir este produto?')) return;
  try {
    await productService.delete(id);
    fetchProducts();
  } catch (error) {
    alert('Erro ao excluir produto');
  }
};

const handleSubmit = async () => {
  try {
    const payload = {
      ...formData.value,
      price: parseFloat(formData.value.price),
      stock: parseInt(formData.value.stock)
    };

    if (editingId.value) {
      await productService.update(editingId.value, payload);
    } else {
      await productService.create(payload);
    }
    isModalOpen.value = false;
    fetchProducts();
  } catch (error) {
    alert('Erro ao salvar produto');
  }
};

watch([page, pageSize, isActiveFilter], fetchProducts);
watch(searchTerm, () => {
  page.value = 1;
  fetchProducts();
});

onMounted(fetchProducts);
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
