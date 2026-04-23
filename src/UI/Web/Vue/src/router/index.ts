import { createRouter, createWebHistory } from 'vue-router';
import Login from '../features/auth/Login.vue';
import Dashboard from '../features/dashboard/Dashboard.vue';
import Products from '../features/products/Products.vue';
import Orders from '../features/orders/Orders.vue';
import Audit from '../features/audit/Audit.vue';
import MainLayout from '../layouts/MainLayout.vue';
import { useAuthStore } from '../store/auth';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { 
      path: '/login', 
      name: 'Login',
      component: Login 
    },
    {
      path: '/',
      component: MainLayout,
      meta: { requiresAuth: true },
      children: [
        { path: 'dashboard', component: Dashboard },
        { path: 'products', component: Products },
        { path: 'orders', component: Orders },
        { path: 'audit', component: Audit },
        { path: '', redirect: '/dashboard' }
      ]
    }
  ]
});

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  if (to.meta.requiresAuth && !authStore.token) {
    next('/login');
  } else if (to.name === 'Login' && authStore.token) {
    next('/dashboard');
  } else {
    next();
  }
});

export default router;
