import { Injectable, signal } from '@angular/core';
import { BaseService } from './base.service';
import { AuthResponseDto } from '../../shared/models/models';
import { tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService extends BaseService {
  currentUser = signal<AuthResponseDto | null>(null);

  login(credentials: any) {
    return this.http.post<AuthResponseDto>(`${this.baseUrl}/api/auth/login`, credentials).pipe(
      tap(response => {
        localStorage.setItem('auth_token', response.token);
        this.currentUser.set(response);
      })
    );
  }

  logout() {
    localStorage.removeItem('auth_token');
    this.currentUser.set(null);
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('auth_token');
  }
}
