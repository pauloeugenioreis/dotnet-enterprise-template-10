import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { PagedResponse, ProductResponse, CreateProductRequest } from '../../shared/models';

@Injectable({ providedIn: 'root' })
export class ProductService extends BaseService<ProductResponse, CreateProductRequest> {
  protected override entityPath = 'api/v1/Product';

  getProducts(page = 1, pageSize = 10, search = '', isActive?: boolean) {
    let url = `${this.fullUrl}?page=${page}&pageSize=${pageSize}`;
    if (search) url += `&searchTerm=${encodeURIComponent(search)}`;
    if (isActive !== undefined) url += `&isActive=${isActive}`;
    return this.http.get<PagedResponse<ProductResponse>>(url);
  }

  exportToExcel(search = '', isActive?: boolean) {
    let url = `${this.fullUrl}/ExportToExcel?`;
    if (search) url += `&searchTerm=${encodeURIComponent(search)}`;
    if (isActive !== undefined) url += `&isActive=${isActive}`;
    return this.http.get(url, { responseType: 'blob' });
  }
}
