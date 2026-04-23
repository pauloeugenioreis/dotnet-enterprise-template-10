import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/pages/login.component';
import { DashboardComponent } from './features/dashboard/pages/dashboard.component';
import { AuditComponent } from './features/audit/pages/audit.component';
import { ProductsComponent } from './features/products/pages/products.component';
import { OrdersComponent } from './features/orders/pages/orders.component';
import { DocumentsComponent } from './features/documents/pages/documents.component';
import { MainLayoutComponent } from './shared/components/layout/main-layout.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { 
    path: '', 
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'audit', component: AuditComponent },
      { path: 'products', component: ProductsComponent },
      { path: 'orders', component: OrdersComponent },
      { path: 'documents', component: DocumentsComponent },
      { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
    ]
  },
  { path: '**', redirectTo: '/login' }
];
