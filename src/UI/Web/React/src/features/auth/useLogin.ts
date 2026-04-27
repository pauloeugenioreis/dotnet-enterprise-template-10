import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../store/useAuthStore';
import { authService } from '../../api/services/auth.service';
import { notify } from '../../utils/toast';

export function useLogin() {
  const [email, setEmail] = useState('admin@projecttemplate.com');
  const [password, setPassword] = useState('Admin@2026!Secure');
  const [loading, setLoading] = useState(false);
  const setAuth = useAuthStore((state) => state.setAuth);
  const navigate = useNavigate();

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const data = await authService.login({ email, password });
      setAuth(data.user, data.accessToken);
      notify.success(`Bem-vindo, ${data.user.firstName}!`, 'Login realizado com sucesso.');
      navigate('/dashboard');
    } catch (error) {
      // Erro tratado no interceptor global
    } finally {
      setLoading(false);
    }
  };

  return {
    email,
    setEmail,
    password,
    setPassword,
    loading,
    handleLogin
  };
}
