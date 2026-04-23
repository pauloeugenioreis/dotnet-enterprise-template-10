import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, Router, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './main-layout.component.html'
})
export class MainLayoutComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  user = this.authService.currentUser;

  menuItems = [
    { name: 'Dashboard', path: '/dashboard', icon: '📊' },
    { name: 'Produtos', path: '/products', icon: '📦' },
    { name: 'Pedidos', path: '/orders', icon: '🛍️' },
    { name: 'Auditoria', path: '/audit', icon: '🛡️' },
    { name: 'Documentos', path: '/documents', icon: '📄' },
  ];

  handleLogout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
