import { AxiosInstance } from 'axios';
import apiClient from './apiClient';
import { PagedResponse } from '../types';

export abstract class BaseService<TResponse, TCreate = any, TUpdate = any> {
  protected http: AxiosInstance = apiClient;

  constructor(protected readonly entityPath: string) {}

  /**
   * Converte um objeto de filtros em uma Query String limpa.
   */
  protected toQueryString(params: Record<string, any>): string {
    const searchParams = new URLSearchParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        // Formata datas para ISO string se necessário
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
  list(page = 1, pageSize = 10, filters: Record<string, any> = {}) {
    const query = this.toQueryString({ page, pageSize, ...filters });
    return this.http.get<PagedResponse<TResponse>>(`${this.entityPath}${query}`).then(res => res.data);
  }

  /**
   * Obtém um item pelo ID.
   */
  getById(id: string | number) {
    return this.http.get<TResponse>(`${this.entityPath}/${id}`).then(res => res.data);
  }

  /**
   * Cria um novo item.
   */
  create(data: TCreate) {
    return this.http.post<TResponse>(this.entityPath, data).then(res => res.data);
  }

  /**
   * Atualiza um item existente.
   */
  update(id: string | number, data: TUpdate) {
    return this.http.put<TResponse>(`${this.entityPath}/${id}`, data).then(res => res.data);
  }

  /**
   * Remove um item.
   */
  delete(id: string | number) {
    return this.http.delete(`${this.entityPath}/${id}`).then(res => res.data);
  }
}
