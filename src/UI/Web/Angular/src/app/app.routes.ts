import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/pages/login.component';
import { DashboardComponent } from './features/dashboard/pages/dashboard.component';
import { AuditComponent } from './features/audit/pages/audit.component';
import { ProductsComponent } from './features/products/pages/products.component';
import { OrdersComponent } from './features/orders/pages/orders.component';
import { DocumentsComponent } from './features/documents/pages/documents.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'audit', component: AuditComponent },
  { path: 'products', component: ProductsComponent },
  { path: 'orders', component: OrdersComponent },
  { path: 'documents', component: DocumentsComponent },
  { path: '', redirectTo: '/login', pathMatch: 'full' }
];
