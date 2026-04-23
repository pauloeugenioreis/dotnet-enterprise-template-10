import axios from 'axios';

import { useLoadingStore } from '../store/loading';

import { config } from '../config';

const api = axios.create({
  baseURL: config.apiBaseUrl,
});

api.interceptors.request.use((config) => {
  const loadingStore = useLoadingStore();
  loadingStore.show();

  const token = localStorage.getItem('auth_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
}, (error) => {
  const loadingStore = useLoadingStore();
  loadingStore.hide();
  return Promise.reject(error);
});

api.interceptors.response.use((response) => {
  const loadingStore = useLoadingStore();
  loadingStore.hide();
  return response;
}, (error) => {
  const loadingStore = useLoadingStore();
  loadingStore.hide();
  return Promise.reject(error);
});

export default api;
