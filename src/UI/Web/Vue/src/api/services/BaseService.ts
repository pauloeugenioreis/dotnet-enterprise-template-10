import api from '../api';
import { AxiosResponse } from 'axios';

export abstract class BaseService<T> {
  protected readonly resourcePath: string;

  constructor(resourcePath: string) {
    this.resourcePath = resourcePath;
  }

  async getAll(params?: any): Promise<any> {
    const response = await api.get(this.resourcePath, { params });
    return response.data;
  }

  async getById(id: number | string): Promise<T> {
    const response = await api.get(`${this.resourcePath}/${id}`);
    return response.data;
  }

  async create(data: any): Promise<T> {
    const response = await api.post(this.resourcePath, data);
    return response.data;
  }

  async update(id: number | string, data: any): Promise<void> {
    await api.put(`${this.resourcePath}/${id}`, data);
  }

  async delete(id: number | string): Promise<void> {
    await api.delete(`${this.resourcePath}/${id}`);
  }

  async patch(id: number | string, data: any): Promise<void> {
    await api.patch(`${this.resourcePath}/${id}`, data);
  }
}
