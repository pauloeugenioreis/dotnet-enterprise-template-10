import apiClient from './apiClient';

export interface OrderItem {
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface OrderResponse {
  id: number;
  orderNumber: string;
  customerName: string;
  customerEmail: string;
  shippingAddress: string;
  status: string;
  subtotal: number;
  shippingCost: number;
  tax: number;
  total: number;
  createdAt: string;
  items: OrderItem[];
}

export const authService = {
  login: (credentials: any) => apiClient.post('/api/auth/login', credentials),
};

export const orderService = {
  getOrders: (page = 1, pageSize = 10, status?: string, searchTerm?: string, startDate?: string, endDate?: string) => {
    let url = `/api/v1/Order?page=${page}&pageSize=${pageSize}`;
    if (status) url += `&status=${status}`;
    if (searchTerm) url += `&searchTerm=${encodeURIComponent(searchTerm)}`;
    if (startDate) url += `&startDate=${startDate}`;
    if (endDate) url += `&endDate=${endDate}`;
    return apiClient.get(url).then(res => res.data);
  },
  createOrder: (order: any) => apiClient.post('/api/v1/Order', order),
  updateStatus: (id: number, status: string, reason: string = 'Atualizado via Dashboard') => apiClient.patch(`/api/v1/Order/${id}/status`, { status, reason }),
  deleteOrder: (id: number) => apiClient.post(`/api/v1/Order/${id}/cancel`),
  exportToExcel: (status?: string, searchTerm?: string, startDate?: string, endDate?: string) => {
    let url = '/api/v1/Order/ExportToExcel?';
    if (status) url += `&status=${status}`;
    if (searchTerm) url += `&searchTerm=${encodeURIComponent(searchTerm)}`;
    if (startDate) url += `&startDate=${startDate}`;
    if (endDate) url += `&endDate=${endDate}`;
    return apiClient.get(url, { responseType: 'blob' });
  },
  getStatistics: () => apiClient.get('/api/v1/Order/statistics').then(res => res.data),
};

export const productService = {
  getProducts: (page = 1, pageSize = 10, searchTerm?: string, isActive?: boolean) => {
    let url = `/api/v1/Product?page=${page}&pageSize=${pageSize}`;
    if (searchTerm) url += `&searchTerm=${encodeURIComponent(searchTerm)}`;
    if (isActive !== undefined) url += `&isActive=${isActive}`;
    return apiClient.get(url).then(res => res.data);
  },
  createProduct: (product: any) => apiClient.post('/api/v1/Product', product),
  updateProduct: (id: number, product: any) => apiClient.put(`/api/v1/Product/${id}`, product),
  deleteProduct: (id: number) => apiClient.delete(`/api/v1/Product/${id}`),
  exportToExcel: (searchTerm?: string, isActive?: boolean) => {
    let url = '/api/v1/Product/ExportToExcel?';
    if (searchTerm) url += `&searchTerm=${encodeURIComponent(searchTerm)}`;
    if (isActive !== undefined) url += `&isActive=${isActive}`;
    return apiClient.get(url, { responseType: 'blob' });
  },
};

export const auditService = {
  getAuditLogs: (page = 1, pageSize = 10, entityType?: string, eventType?: string, userId?: string, from?: string, toDate?: string) => {
    let url = `/api/v1/audit?page=${page}&pageSize=${pageSize}`;
    if (entityType) url = `/api/v1/audit/${entityType}?page=${page}&pageSize=${pageSize}`;
    if (eventType) url += `&eventType=${eventType}`;
    if (userId) url += `&userId=${userId}`;
    if (from) url += `&from=${from}`;
    if (toDate) url += `&toDate=${toDate}`;
    return apiClient.get(url).then(res => res.data);
  },
};
export const documentService = {
  upload: (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return apiClient.post('/api/v1/Document/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }).then(res => res.data);
  },
  download: (fileName: string) => apiClient.get(`/api/v1/Document/${fileName}`, { responseType: 'blob' }),
  delete: (fileName: string) => apiClient.delete(`/api/v1/Document/${fileName}`),
};
