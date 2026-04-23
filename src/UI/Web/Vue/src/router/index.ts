import { createRouter, createWebHistory } from 'vue-router';
import Login from '../features/auth/Login.vue';
import Dashboard from '../features/dashboard/Dashboard.vue';
import Products from '../features/products/Products.vue';
import Orders from '../features/orders/Orders.vue';
import Audit from '../features/audit/Audit.vue';
import MainLayout from '../layouts/MainLayout.vue';
import { useAuthStore } from '../store/auth';

import { authGuard } from './guards/authGuard';

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
        { path: 'dashboard', name: 'Dashboard', component: Dashboard },
        { path: 'products', name: 'Products', component: Products },
        { path: 'orders', name: 'Orders', component: Orders },
        { path: 'audit', name: 'Audit', component: Audit },
        { path: '', redirect: '/dashboard' }
      ]
    }
  ]
});

router.beforeEach(authGuard);

export default router;
