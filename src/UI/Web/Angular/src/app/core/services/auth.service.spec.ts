import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { StorageService } from './storage.service';
import { AuthResponse, User } from '../../shared/models';

const mockUser: User = {
  id: 'u1',
  email: 'admin@test.com',
  firstName: 'Admin',
  lastName: 'User',
  fullName: 'Admin User',
  profileImageUrl: null,
  roles: ['admin'],
  createdAt: '2026-01-01T00:00:00Z'
};

const mockAuthResponse: AuthResponse = {
  accessToken: 'jwt-token-abc',
  refreshToken: 'refresh-token-xyz',
  expiresAt: '2026-12-31T00:00:00Z',
  user: mockUser
};

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  const mockStorage = {
    get: vi.fn(),
    set: vi.fn(),
    remove: vi.fn(),
    getObject: vi.fn(),
    setObject: vi.fn()
  };

  function setup(storedToken: string | null = null, storedUser: User | null = null) {
    mockStorage.get.mockReturnValue(storedToken);
    mockStorage.getObject.mockReturnValue(storedUser);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: StorageService, useValue: mockStorage }
      ]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  }

  afterEach(() => {
    vi.clearAllMocks();
    TestBed.resetTestingModule();
  });

  describe('constructor — restoring session from storage', () => {
    it('should set currentUser signal when token and user exist in storage', () => {
      setup('jwt-token-abc', mockUser);
      expect(service.currentUser()).toEqual(mockUser);
    });

    it('should leave currentUser as null when storage is empty', () => {
      setup(null, null);
      expect(service.currentUser()).toBeNull();
    });

    it('should leave currentUser as null when token is present but user is missing', () => {
      setup('jwt-token-abc', null);
      expect(service.currentUser()).toBeNull();
    });
  });

  describe('login()', () => {
    beforeEach(() => setup());

    it('should POST to the auth login endpoint', () => {
      service.login({ email: 'admin@test.com', password: 'secret' }).subscribe();
      const req = httpMock.expectOne(r => r.url.includes('/api/auth/login'));
      expect(req.request.method).toBe('POST');
      req.flush(mockAuthResponse);
    });

    it('should store the access token in storage', () => {
      service.login({ email: 'admin@test.com', password: 'secret' }).subscribe();
      httpMock.expectOne(r => r.url.includes('/api/auth/login')).flush(mockAuthResponse);
      expect(mockStorage.set).toHaveBeenCalledWith('auth_token', 'jwt-token-abc');
    });

    it('should store the user object in storage', () => {
      service.login({ email: 'admin@test.com', password: 'secret' }).subscribe();
      httpMock.expectOne(r => r.url.includes('/api/auth/login')).flush(mockAuthResponse);
      expect(mockStorage.setObject).toHaveBeenCalledWith('auth_user', mockUser);
    });

    it('should update the currentUser signal on success', () => {
      service.login({ email: 'admin@test.com', password: 'secret' }).subscribe();
      httpMock.expectOne(r => r.url.includes('/api/auth/login')).flush(mockAuthResponse);
      expect(service.currentUser()).toEqual(mockUser);
    });
  });

  describe('logout()', () => {
    it('should remove the token and user from storage', () => {
      setup('jwt-token-abc', mockUser);
      service.logout();
      expect(mockStorage.remove).toHaveBeenCalledWith('auth_token');
      expect(mockStorage.remove).toHaveBeenCalledWith('auth_user');
    });

    it('should clear the currentUser signal', () => {
      setup('jwt-token-abc', mockUser);
      service.logout();
      expect(service.currentUser()).toBeNull();
    });
  });

  describe('isAuthenticated()', () => {
    it('should return true when a token is stored', () => {
      setup('jwt-token-abc', null);
      expect(service.isAuthenticated()).toBe(true);
    });

    it('should return false when no token is stored', () => {
      setup(null, null);
      expect(service.isAuthenticated()).toBe(false);
    });
  });

  describe('getToken()', () => {
    it('should return the stored token', () => {
      setup('jwt-token-abc', null);
      expect(service.getToken()).toBe('jwt-token-abc');
    });

    it('should return null when no token exists', () => {
      setup(null, null);
      expect(service.getToken()).toBeNull();
    });
  });
});
