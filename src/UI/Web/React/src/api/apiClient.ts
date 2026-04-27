import axios from 'axios';
import { notify } from '../utils/toast';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7196',
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('auth_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;
    const data = error.response?.data;

    if (status === 401) {
      notify.error('Sessão Expirada', 'Por favor, faça login novamente.');
      localStorage.removeItem('auth_token');
      localStorage.removeItem('auth_user');
      window.location.href = '/login';
    } else if (status === 400 || status === 422) {
      const message = data?.title || data?.message || 'Erro na requisição';
      notify.error('Requisição Inválida', message);
    } else if (status >= 500) {
      notify.error('Erro no Servidor', 'Ocorreu uma falha interna no sistema.');
    } else if (error.code === 'ERR_NETWORK') {
      notify.error('Falha de Conexão', 'Não foi possível conectar ao servidor da API.');
    }

    return Promise.reject(error);
  }
);

export default apiClient;
