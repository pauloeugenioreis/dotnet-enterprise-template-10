import axios from 'axios';
import * as SecureStore from 'expo-secure-store';

const API_URL = 'https://localhost:7196'; // Em dev com emulador Android, usar 10.0.2.2

const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  },
});

// Interceptor para injetar o Token JWT (Equivalente ao AuthTokenHandler)
apiClient.interceptors.request.use(async (config) => {
  const token = await SecureStore.getItemAsync('auth_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default apiClient;
