<template src="./Products.html"></template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { computed } from 'vue';
import { useProducts } from '../../composables/api/useProducts';
import { productService } from '../../api/services/ProductService';
import Pagination from '../../components/Pagination.vue';
import Modal from '../../components/Modal.vue';

const { 
  products, 
  loading, 
  totalCount, 
  fetchProducts, 
  deleteProduct, 
  createProduct 
} = useProducts();

const page = ref(1);
const pageSize = ref(10);
const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value));
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

const handleFetchProducts = () => {
  fetchProducts(page.value, pageSize.value, {
    searchTerm: searchTerm.value,
    isActive: isActiveFilter.value
  });
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
    await deleteProduct(id);
    handleFetchProducts();
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

watch([page, pageSize, isActiveFilter], handleFetchProducts);
watch(searchTerm, () => {
  page.value = 1;
  handleFetchProducts();
});

onMounted(handleFetchProducts);
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
