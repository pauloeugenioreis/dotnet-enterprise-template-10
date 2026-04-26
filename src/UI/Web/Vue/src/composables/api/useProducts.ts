import { ref } from 'vue';
import { productService } from '../../api/services/ProductService';
import { ProductResponse, CreateProductRequest } from '../../types';

export function useProducts() {
  const products = ref<ProductResponse[]>([]);
  const totalCount = ref(0);
  const loading = ref(false);

  const fetchProducts = async (page = 1, pageSize = 10, filters = {}) => {
    loading.value = true;
    try {
      const data = await productService.list(page, pageSize, filters);
      products.value = data.items;
      totalCount.value = data.totalCount;
      return data;
    } finally {
      loading.value = false;
    }
  };

  const deleteProduct = async (id: number | string) => {
    await productService.delete(id);
  };

  const createProduct = async (product: CreateProductRequest) => {
    return await productService.create(product);
  };

  return {
    products,
    totalCount,
    loading,
    fetchProducts,
    deleteProduct,
    createProduct
  };
}
