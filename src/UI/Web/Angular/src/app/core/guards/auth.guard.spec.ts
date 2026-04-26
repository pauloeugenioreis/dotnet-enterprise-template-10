import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('authGuard', () => {
  const mockRoute = {} as ActivatedRouteSnapshot;

  const mockAuthService = {
    isAuthenticated: vi.fn()
  };

  const mockRouter = {
    navigate: vi.fn()
  };

  function runGuard(url: string): boolean | unknown {
    const state = { url } as RouterStateSnapshot;
    return TestBed.runInInjectionContext(() => authGuard(mockRoute, state));
  }

  beforeEach(() => {
    vi.clearAllMocks();
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    });
  });

  it('should return true when the user is authenticated', () => {
    mockAuthService.isAuthenticated.mockReturnValue(true);
    expect(runGuard('/dashboard')).toBe(true);
  });

  it('should return false when the user is not authenticated', () => {
    mockAuthService.isAuthenticated.mockReturnValue(false);
    expect(runGuard('/dashboard')).toBe(false);
  });

  it('should redirect to /login when not authenticated', () => {
    mockAuthService.isAuthenticated.mockReturnValue(false);
    runGuard('/orders');
    expect(mockRouter.navigate).toHaveBeenCalledWith(
      ['/login'],
      { queryParams: { returnUrl: '/orders' } }
    );
  });

  it('should not redirect when authenticated', () => {
    mockAuthService.isAuthenticated.mockReturnValue(true);
    runGuard('/products');
    expect(mockRouter.navigate).not.toHaveBeenCalled();
  });

  it('should pass the attempted URL as returnUrl query param', () => {
    mockAuthService.isAuthenticated.mockReturnValue(false);
    runGuard('/reviews?page=2');
    expect(mockRouter.navigate).toHaveBeenCalledWith(
      ['/login'],
      { queryParams: { returnUrl: '/reviews?page=2' } }
    );
  });
});
