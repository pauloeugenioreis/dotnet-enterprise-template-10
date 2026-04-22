import apiClient from './apiClient';

export const authService = {
  login: (credentials: any) => apiClient.post('/api/auth/login', credentials),
};

export const orderService = {
  getOrders: (page = 1, pageSize = 10) => 
    apiClient.get(`/api/v1/Order?page=${page}&pageSize=${pageSize}`).then(res => res.data),
  createOrder: (order: any) => apiClient.post('/api/v1/Order', order),
  updateStatus: (id: number, status: string) => apiClient.patch(`/api/v1/Order/${id}/status`, { status }),
  deleteOrder: (id: number) => apiClient.post(`/api/v1/Order/${id}/cancel`),
  exportToExcel: () => apiClient.get('/api/v1/Order/ExportToExcel', { responseType: 'blob' }),
};

export const productService = {
  getProducts: (page = 1, pageSize = 10) => 
    apiClient.get(`/api/v1/Product?page=${page}&pageSize=${pageSize}`).then(res => res.data),
  createProduct: (product: any) => apiClient.post('/api/v1/Product', product),
  updateProduct: (id: number, product: any) => apiClient.put(`/api/v1/Product/${id}`, product),
  deleteProduct: (id: number) => apiClient.delete(`/api/v1/Product/${id}`),
  exportToExcel: () => apiClient.get('/api/v1/Product/ExportToExcel', { responseType: 'blob' }),
};

export const auditService = {
  getAuditLogs: (entityType: string, page = 1, pageSize = 10) => 
    apiClient.get(`/api/v1/audit/${entityType}?page=${page}&pageSize=${pageSize}`).then(res => res.data),
};
