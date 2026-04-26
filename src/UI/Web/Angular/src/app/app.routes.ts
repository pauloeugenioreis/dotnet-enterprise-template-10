import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/pages/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./shared/components/layout/main-layout.component').then(m => m.MainLayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/pages/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'audit',
        loadComponent: () => import('./features/audit/pages/audit.component').then(m => m.AuditComponent)
      },
      {
        path: 'products',
        loadComponent: () => import('./features/products/pages/products.component').then(m => m.ProductsComponent)
      },
      {
        path: 'orders',
        loadComponent: () => import('./features/orders/pages/orders.component').then(m => m.OrdersComponent)
      },
      {
        path: 'documents',
        loadComponent: () => import('./features/documents/pages/documents.component').then(m => m.DocumentsComponent)
      },
      {
        path: 'reviews',
        loadComponent: () => import('./features/reviews/pages/reviews.component').then(m => m.ReviewsComponent)
      },
      { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
    ]
  },
  { path: '**', redirectTo: '/login' }
];
