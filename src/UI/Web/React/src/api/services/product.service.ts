import { BaseService } from '../base.service';
import { ProductResponse, CreateProductRequest } from '../../types';

export class ProductService extends BaseService<ProductResponse, CreateProductRequest, CreateProductRequest> {
  constructor() {
    super('/api/v1/Product');
  }

  async exportToExcel(filters: Record<string, any> = {}) {
    const query = this.toQueryString(filters);
    const response = await this.http.get(`${this.entityPath}/ExportToExcel${query}`, {
      responseType: 'blob'
    });
    return response.data;
  }
}

export const productService = new ProductService();
