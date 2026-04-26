import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { errorInterceptor } from './error.interceptor';
import { NotificationService } from '../services/notification.service';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;

  const mockNotification = {
    error: vi.fn()
  };

  beforeEach(() => {
    vi.clearAllMocks();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: NotificationService, useValue: mockNotification }
      ]
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  function makeRequest(status: number, body: object | null = null) {
    http.get('/api/test').subscribe({ error: () => {} });
    httpMock.expectOne('/api/test').flush(body, { status, statusText: String(status) });
  }

  describe('default error messages by HTTP status', () => {
    it('should show "Requisição inválida" for 400', () => {
      makeRequest(400);
      expect(mockNotification.error).toHaveBeenCalledWith(
        'Requisição inválida. Verifique os dados informados.'
      );
    });

    it('should show "Acesso negado" for 403', () => {
      makeRequest(403);
      expect(mockNotification.error).toHaveBeenCalledWith('Acesso negado.');
    });

    it('should show "Recurso não encontrado" for 404', () => {
      makeRequest(404);
      expect(mockNotification.error).toHaveBeenCalledWith('Recurso não encontrado.');
    });

    it('should show "Conflito" for 409', () => {
      makeRequest(409);
      expect(mockNotification.error).toHaveBeenCalledWith(
        'Conflito: o recurso já existe ou foi modificado.'
      );
    });

    it('should show "Dados inválidos" for 422', () => {
      makeRequest(422);
      expect(mockNotification.error).toHaveBeenCalledWith('Dados inválidos.');
    });

    it('should show "Erro interno do servidor" for 500', () => {
      makeRequest(500);
      expect(mockNotification.error).toHaveBeenCalledWith(
        'Erro interno do servidor. Tente novamente.'
      );
    });

    it('should show "Sem conexão" for status 0', () => {
      makeRequest(0);
      expect(mockNotification.error).toHaveBeenCalledWith('Sem conexão com o servidor.');
    });

    it('should show "Ocorreu um erro inesperado" for unknown status codes', () => {
      makeRequest(418);
      expect(mockNotification.error).toHaveBeenCalledWith('Ocorreu um erro inesperado.');
    });
  });

  describe('custom error messages from response body', () => {
    it('should prefer error.message over the default message', () => {
      http.get('/api/test').subscribe({ error: () => {} });
      httpMock.expectOne('/api/test').flush(
        { message: 'Email já cadastrado' },
        { status: 400, statusText: 'Bad Request' }
      );
      expect(mockNotification.error).toHaveBeenCalledWith('Email já cadastrado');
    });

    it('should use error.title when error.message is absent', () => {
      http.get('/api/test').subscribe({ error: () => {} });
      httpMock.expectOne('/api/test').flush(
        { title: 'Validation Failed' },
        { status: 422, statusText: 'Unprocessable Entity' }
      );
      expect(mockNotification.error).toHaveBeenCalledWith('Validation Failed');
    });
  });

  describe('401 passthrough', () => {
    it('should NOT call notification.error for 401 (handled by authInterceptor)', () => {
      http.get('/api/secure').subscribe({ error: () => {} });
      httpMock.expectOne('/api/secure').flush(
        { message: 'Unauthorized' },
        { status: 401, statusText: 'Unauthorized' }
      );
      expect(mockNotification.error).not.toHaveBeenCalled();
    });
  });

  describe('error propagation', () => {
    it('should re-throw the error after notifying', () => {
      let capturedStatus = 0;
      http.get('/api/test').subscribe({ error: err => { capturedStatus = err.status; } });
      httpMock.expectOne('/api/test').flush(null, { status: 404, statusText: 'Not Found' });

      expect(capturedStatus).toBe(404);
    });
  });
});
