import { BaseService } from './BaseService';

export class ProductService extends BaseService<any> {
  constructor() {
    super('/api/v1/product');
  }

  async exportToExcel(): Promise<void> {
    window.open(`${import.meta.env.VITE_API_BASE_URL || 'https://localhost:7196'}${this.resourcePath}/export`, '_blank');
  }
}

export const productService = new ProductService();
