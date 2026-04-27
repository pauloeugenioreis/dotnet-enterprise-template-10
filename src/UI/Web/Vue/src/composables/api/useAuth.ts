import { ref } from 'vue';
import { useToast } from 'vue-toastification';
import { authService } from '../../api/services/AuthService';
import { LoginRequest } from '../../types';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../../store/auth';

export function useAuth() {
  const loading = ref(false);
  const router = useRouter();
  const toast = useToast();

  const login = async (credentials: LoginRequest) => {
    loading.value = true;
    try {
      const data = await authService.login(credentials);
      const authStore = useAuthStore();
      authStore.setAuth(data.user, data.accessToken);
      toast.success(`Bem-vindo, ${data.user.firstName}!`);
      router.push('/dashboard');
      return data;
    } finally {
      loading.value = false;
    }
  };

  const logout = () => {
    const authStore = useAuthStore();
    authStore.logout();
    router.push('/login');
  };

  return {
    loading,
    login,
    logout
  };
}
