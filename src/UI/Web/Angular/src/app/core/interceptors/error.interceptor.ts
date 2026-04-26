import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notification = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // 401 is handled by authInterceptor (logout + redirect)
      if (error.status !== 401) {
        const message =
          error.error?.message ||
          error.error?.title ||
          getDefaultMessage(error.status);
        notification.error(message);
      }
      return throwError(() => error);
    })
  );
};

function getDefaultMessage(status: number): string {
  switch (status) {
    case 400: return 'Requisição inválida. Verifique os dados informados.';
    case 403: return 'Acesso negado.';
    case 404: return 'Recurso não encontrado.';
    case 409: return 'Conflito: o recurso já existe ou foi modificado.';
    case 422: return 'Dados inválidos.';
    case 500: return 'Erro interno do servidor. Tente novamente.';
    case 0:   return 'Sem conexão com o servidor.';
    default:  return 'Ocorreu um erro inesperado.';
  }
}
