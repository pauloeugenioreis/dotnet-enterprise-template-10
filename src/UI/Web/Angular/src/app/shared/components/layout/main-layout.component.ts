import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, Router, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <div class="flex h-screen bg-gray-100">
      <!-- Sidebar -->
      <div class="w-64 bg-white shadow-lg flex flex-col">
        <div class="p-6 border-b flex items-center gap-3">
          <div class="w-10 h-10 bg-primary-600 rounded-lg flex items-center justify-center text-white font-bold text-xl">
            ET
          </div>
          <span class="font-bold text-xl text-gray-800">Enterprise</span>
        </div>
        
        <nav class="mt-6 px-4 flex-1 space-y-2">
          @for (item of menuItems; track item.path) {
            <a
              [routerLink]="item.path"
              routerLinkActive="bg-primary-50 text-primary-600"
              [routerLinkActiveOptions]="{exact: true}"
              class="flex items-center gap-3 px-4 py-3 rounded-lg transition-colors text-gray-600 hover:bg-gray-50"
            >
              <span>{{ item.icon }}</span>
              <span class="font-medium">{{ item.name }}</span>
            </a>
          }
        </nav>

        <div class="p-6 border-t bg-white">
          <div class="flex items-center gap-3 mb-4">
            <div class="w-10 h-10 bg-gray-200 rounded-full flex items-center justify-center text-gray-600 font-bold">
              {{ user()?.firstName?.[0] || 'A' }}
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-semibold text-gray-800 truncate">
                {{ user()?.firstName }} {{ user()?.lastName }}
              </p>
              <p class="text-xs text-gray-500 truncate">{{ user()?.email }}</p>
            </div>
          </div>
          <button
            (click)="handleLogout()"
            class="w-full flex items-center justify-center gap-2 px-4 py-2 border border-red-200 text-red-600 rounded-lg hover:bg-red-50 transition-colors text-sm font-medium"
          >
            🚪 Sair
          </button>
        </div>
      </div>

      <div class="flex-1 overflow-auto">
        <router-outlet />
      </div>
    </div>
  `
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
