import { ref } from 'vue';
import { authService } from '../../api/services/AuthService';
import { LoginRequest } from '../../types';
import { useRouter } from 'vue-router';

export function useAuth() {
  const loading = ref(false);
  const router = useRouter();

  const login = async (credentials: LoginRequest) => {
    loading.value = true;
    try {
      const data = await authService.login(credentials);
      localStorage.setItem('auth_token', data.accessToken);
      localStorage.setItem('auth_user', JSON.stringify(data.user));
      router.push('/dashboard');
      return data;
    } finally {
      loading.value = false;
    }
  };

  const logout = () => {
    authService.logout();
    router.push('/login');
  };

  return {
    loading,
    login,
    logout
  };
}
