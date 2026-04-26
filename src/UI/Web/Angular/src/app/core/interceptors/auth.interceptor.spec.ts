import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from '../services/auth.service';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;

  const mockAuthService = {
    getToken: vi.fn(),
    logout: vi.fn()
  };

  const mockRouter = {
    navigate: vi.fn()
  };

  beforeEach(() => {
    vi.clearAllMocks();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  describe('Authorization header', () => {
    it('should attach Bearer token when authenticated', () => {
      mockAuthService.getToken.mockReturnValue('my-jwt-token');

      http.get('/api/products').subscribe();
      const req = httpMock.expectOne('/api/products');

      expect(req.request.headers.get('Authorization')).toBe('Bearer my-jwt-token');
      req.flush({});
    });

    it('should not attach Authorization header when no token exists', () => {
      mockAuthService.getToken.mockReturnValue(null);

      http.get('/api/products').subscribe();
      const req = httpMock.expectOne('/api/products');

      expect(req.request.headers.has('Authorization')).toBe(false);
      req.flush({});
    });
  });

  describe('401 Unauthorized handling', () => {
    it('should call logout and redirect to /login on 401 for protected endpoints', () => {
      mockAuthService.getToken.mockReturnValue('expired-token');

      http.get('/api/products').subscribe({ error: () => {} });
      const req = httpMock.expectOne('/api/products');
      req.flush({ message: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });

      expect(mockAuthService.logout).toHaveBeenCalled();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
    });

    it('should NOT redirect on 401 for the login endpoint', () => {
      mockAuthService.getToken.mockReturnValue(null);

      http.post('/api/auth/login', {}).subscribe({ error: () => {} });
      const req = httpMock.expectOne('/api/auth/login');
      req.flush({ message: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });

      expect(mockAuthService.logout).not.toHaveBeenCalled();
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should NOT redirect on non-401 errors', () => {
      mockAuthService.getToken.mockReturnValue('token');

      http.get('/api/orders').subscribe({ error: () => {} });
      const req = httpMock.expectOne('/api/orders');
      req.flush({ message: 'Not Found' }, { status: 404, statusText: 'Not Found' });

      expect(mockAuthService.logout).not.toHaveBeenCalled();
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });

  describe('error propagation', () => {
    it('should re-throw the error so other handlers can catch it', () => {
      mockAuthService.getToken.mockReturnValue('token');

      let capturedStatus = 0;
      http.get('/api/products').subscribe({ error: err => { capturedStatus = err.status; } });
      httpMock.expectOne('/api/products').flush({}, { status: 500, statusText: 'Server Error' });

      expect(capturedStatus).toBe(500);
    });
  });
});
