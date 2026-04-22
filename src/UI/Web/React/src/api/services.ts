import apiClient from './apiClient';

export const authService = {
  login: (credentials: any) => apiClient.post('/api/auth/login', credentials),
};

export const orderService = {
  getOrders: (page = 1, pageSize = 10) => 
    apiClient.get(`/api/orders?page=${page}&pageSize=${pageSize}`).then(res => res.data),
};

export const productService = {
  getProducts: (page = 1, pageSize = 10) => 
    apiClient.get(`/api/products?page=${page}&pageSize=${pageSize}`).then(res => res.data),
};

export const auditService = {
  getAuditLogs: (entityType: string, page = 1, pageSize = 10) => 
    apiClient.get(`/api/v1/audit/${entityType}?page=${page}&pageSize=${pageSize}`).then(res => res.data),
};
