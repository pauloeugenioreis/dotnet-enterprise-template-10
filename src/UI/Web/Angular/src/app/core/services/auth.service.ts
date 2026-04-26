import { Injectable, inject, signal } from '@angular/core';
import { BaseService } from './base.service';
import { StorageService } from './storage.service';
import { AuthResponse, LoginRequest, User } from '../../shared/models';
import { tap } from 'rxjs';

const TOKEN_KEY = 'auth_token';
const USER_KEY = 'auth_user';

@Injectable({ providedIn: 'root' })
export class AuthService extends BaseService {
  protected override entityPath = 'api/auth';
  private storage = inject(StorageService);

  currentUser = signal<User | null>(null);

  constructor() {
    super();
    const token = this.storage.get(TOKEN_KEY);
    const user = this.storage.getObject<User>(USER_KEY);
    if (token && user) {
      this.currentUser.set(user);
    }
  }

  login(credentials: LoginRequest) {
    return this.http.post<AuthResponse>(`${this.fullUrl}/login`, credentials).pipe(
      tap(response => {
        this.storage.set(TOKEN_KEY, response.accessToken);
        this.storage.setObject(USER_KEY, response.user);
        this.currentUser.set(response.user);
      })
    );
  }

  logout() {
    this.storage.remove(TOKEN_KEY);
    this.storage.remove(USER_KEY);
    this.currentUser.set(null);
  }

  isAuthenticated(): boolean {
    return !!this.storage.get(TOKEN_KEY);
  }

  getToken(): string | null {
    return this.storage.get(TOKEN_KEY);
  }
}
