import axios from 'axios';
import { useToast } from 'vue-toastification';
import { useLoadingStore } from '../store/loading';
import { config } from '../config';

const api = axios.create({
  baseURL: config.apiBaseUrl,
});

const toast = useToast();

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
  
  const status = error.response?.status;
  const data = error.response?.data;

  if (status === 401) {
    toast.error('Sessão Expirada. Por favor, faça login novamente.');
    localStorage.removeItem('auth_token');
    localStorage.removeItem('auth_user');
    window.location.href = '/login';
  } else if (status === 400 || status === 422) {
    const message = data?.title || data?.message || 'Erro na requisição';
    toast.error(`Requisição Inválida: ${message}`);
  } else if (status >= 500) {
    toast.error('Ocorreu uma falha interna no servidor.');
  } else if (error.code === 'ERR_NETWORK') {
    toast.error('Não foi possível conectar ao servidor da API.');
  }
  
  return Promise.reject(error);
});

export default api;
