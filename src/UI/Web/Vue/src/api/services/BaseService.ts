import api from '../api';
import { PagedResponse } from '../../types';

export abstract class BaseService<TResponse, TCreate = any, TUpdate = any> {
  protected readonly entityPath: string;
  protected http = api;

  constructor(entityPath: string) {
    this.entityPath = entityPath;
  }

  /**
   * Converte um objeto de filtros em uma Query String limpa.
   */
  protected toQueryString(params: Record<string, any>): string {
    const searchParams = new URLSearchParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        const formattedValue = value instanceof Date ? value.toISOString() : value;
        searchParams.append(key, formattedValue.toString());
      }
    });
    const queryString = searchParams.toString();
    return queryString ? `?${queryString}` : '';
  }

  /**
   * Lista itens com paginação e filtros opcionais.
   */
  async list(page = 1, pageSize = 10, filters: Record<string, any> = {}): Promise<PagedResponse<TResponse>> {
    const query = this.toQueryString({ page, pageSize, ...filters });
    const response = await this.http.get<PagedResponse<TResponse>>(`${this.entityPath}${query}`);
    return response.data;
  }

  /**
   * Obtém um item pelo ID.
   */
  async getById(id: number | string): Promise<TResponse> {
    const response = await this.http.get<TResponse>(`${this.entityPath}/${id}`);
    return response.data;
  }

  /**
   * Cria um novo item.
   */
  async create(data: TCreate): Promise<TResponse> {
    const response = await this.http.post<TResponse>(this.entityPath, data);
    return response.data;
  }

  /**
   * Atualiza um item existente.
   */
  async update(id: number | string, data: TUpdate): Promise<void> {
    await this.http.put(`${this.entityPath}/${id}`, data);
  }

  /**
   * Remove um item.
   */
  async delete(id: number | string): Promise<void> {
    await this.http.delete(`${this.entityPath}/${id}`);
  }

  /**
   * Atualização parcial.
   */
  async patch(id: number | string, data: any): Promise<void> {
    await this.http.patch(`${this.entityPath}/${id}`, data);
  }

  /**
   * Utilitário para download de arquivos.
   */
  protected async downloadFile(url: string, fileName: string): Promise<void> {
    const response = await this.http.get(url, { responseType: 'blob' });
    const contentType = response.headers['content-type'] as string;
    const blob = new Blob([response.data], { type: contentType });
    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = fileName;
    link.click();
    window.URL.revokeObjectURL(link.href);
  }
}
