import { BaseService } from './BaseService';

export class ProductService extends BaseService<any> {
  constructor() {
    super('/api/v1/product');
  }

  async exportToExcel(): Promise<void> {
    await this.downloadFile(`${this.resourcePath}/ExportToExcel`, `Produtos_${new Date().getTime()}.xlsx`);
  }
}

export const productService = new ProductService();
