import api from '../api';

export class DocumentService {
  private resourcePath = '/api/v1/document';

  async upload(file: File): Promise<any> {
    const formData = new FormData();
    formData.append('file', file);
    const response = await api.post(`${this.resourcePath}/upload`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  }

  async download(fileName: string): Promise<void> {
    const response = await api.get(`${this.resourcePath}/${fileName}`, { responseType: 'blob' });
    const contentType = (response.headers['content-type'] as string) || 'application/octet-stream';
    const blob = new Blob([response.data], { type: contentType });
    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = fileName;
    link.click();
    window.URL.revokeObjectURL(link.href);
  }

  async delete(fileName: string): Promise<void> {
    await api.delete(`${this.resourcePath}/${fileName}`);
  }
}

export const documentService = new DocumentService();
