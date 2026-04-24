import { Injectable, signal } from '@angular/core';
import { BaseService } from './base.service';
import { AuthResponse, User } from '../../shared/models/models';
import { tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService extends BaseService {
  currentUser = signal<User | null>(null);

  constructor() {
    super();
    const token = localStorage.getItem('auth_token');
    const userStr = localStorage.getItem('auth_user');
    if (token && userStr) {
      try {
        this.currentUser.set(JSON.parse(userStr));
      } catch {
        this.logout();
      }
    }
  }

  login(credentials: any) {
    return this.http.post<AuthResponse>(`${this.baseUrl}/api/auth/login`, credentials).pipe(
      tap(response => {
        localStorage.setItem('auth_token', response.accessToken);
        localStorage.setItem('auth_user', JSON.stringify(response.user));
        this.currentUser.set(response.user);
      })
    );
  }

  logout() {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('auth_user');
    this.currentUser.set(null);
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('auth_token');
  }
}
