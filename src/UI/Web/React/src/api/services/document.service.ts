import apiClient from '../apiClient';

export class DocumentService {
  private readonly entityPath = '/api/v1/Document';

  async upload(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    const response = await apiClient.post(`${this.entityPath}/upload`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  }

  async download(fileName: string) {
    const response = await apiClient.get(`${this.entityPath}/${fileName}`, {
      responseType: 'blob',
    });
    return response.data;
  }

  async delete(fileName: string) {
    return apiClient.delete(`${this.entityPath}/${fileName}`);
  }
}

export const documentService = new DocumentService();
