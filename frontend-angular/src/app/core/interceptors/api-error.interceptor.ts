import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const apiErrorInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        return throwError(() => error);
      }

      return throwError(
        () =>
          new HttpErrorResponse({
            status: 0,
            statusText: 'Unknown Error',
            error: { message: 'Erro inesperado no frontend.' }
          })
      );
    })
  );
