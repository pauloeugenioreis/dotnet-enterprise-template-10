import apiClient from '../apiClient';
import { AuthResponse, LoginRequest } from '../../types';

export class AuthService {
  private readonly entityPath = '/api/auth';

  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>(`${this.entityPath}/login`, credentials);
    return response.data;
  }

  logout(): void {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('auth_user');
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('auth_token');
  }
}

export const authService = new AuthService();
