import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';

export abstract class BaseService<TResponse = any, TCreate = any, TUpdate = any> {
  protected http = inject(HttpClient);
  protected baseUrl = environment.apiUrl;
  protected abstract entityPath: string;

  protected get fullUrl() {
    return `${this.baseUrl}/${this.entityPath}`;
  }

  getById(id: string) {
    return this.http.get<TResponse>(`${this.fullUrl}/${id}`);
  }

  create(item: TCreate) {
    return this.http.post<TResponse>(this.fullUrl, item);
  }

  update(id: string, item: TUpdate) {
    return this.http.put(`${this.fullUrl}/${id}`, item);
  }

  delete(id: string) {
    return this.http.delete(`${this.fullUrl}/${id}`);
  }
}
