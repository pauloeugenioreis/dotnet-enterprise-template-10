import { createRouter, createWebHistory } from 'vue-router';
import Login from '../features/auth/Login.vue';
import Dashboard from '../features/dashboard/Dashboard.vue';
import Products from '../features/products/Products.vue';
import Orders from '../features/orders/Orders.vue';
import Audit from '../features/audit/Audit.vue';
import { useAuthStore } from '../store/auth';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', component: Login },
    { path: '/dashboard', component: Dashboard, meta: { requiresAuth: true } },
    { path: '/products', component: Products, meta: { requiresAuth: true } },
    { path: '/orders', component: Orders, meta: { requiresAuth: true } },
    { path: '/audit', component: Audit, meta: { requiresAuth: true } },
    { path: '/', redirect: '/login' }
  ]
});

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  if (to.meta.requiresAuth && !authStore.token) {
    next('/login');
  } else {
    next();
  }
});

export default router;
