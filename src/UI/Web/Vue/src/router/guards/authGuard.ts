import { NavigationGuardNext, RouteLocationNormalized } from 'vue-router';
import { useAuthStore } from '../../store/auth';

export const authGuard = (to: RouteLocationNormalized, from: RouteLocationNormalized, next: NavigationGuardNext) => {
  const authStore = useAuthStore();

  const requiresAuth = to.matched.some(record => record.meta.requiresAuth);
  const isLoginPage = to.name === 'Login';
  const isAuthenticated = !!authStore.token;

  if (requiresAuth && !isAuthenticated) {
    next({ name: 'Login' });
  } else if (isLoginPage && isAuthenticated) {
    next({ name: 'Dashboard' });
  } else {
    next();
  }
};
