import { BaseService } from './BaseService';
import { ProductResponse, CreateProductRequest } from '../../types';

export class ProductService extends BaseService<ProductResponse, CreateProductRequest, CreateProductRequest> {
  constructor() {
    super('/api/v1/product');
  }

  async exportToExcel(filters: Record<string, any> = {}): Promise<void> {
    const query = this.toQueryString(filters);
    await this.downloadFile(`${this.entityPath}/ExportToExcel${query}`, `Produtos_${new Date().getTime()}.xlsx`);
  }
}

export const productService = new ProductService();
