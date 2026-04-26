import { Injectable } from '@angular/core';
import { BaseService } from './base.service';

@Injectable({ providedIn: 'root' })
export class DocumentService extends BaseService<any> {
  protected override entityPath = 'api/v1/Document';

  upload(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ fileName: string }>(`${this.fullUrl}/upload`, formData);
  }

  download(fileName: string) {
    return this.http.get(`${this.fullUrl}/${fileName}`, { responseType: 'blob' });
  }

  override delete(fileName: string) {
    return this.http.delete(`${this.fullUrl}/${fileName}`);
  }
}
