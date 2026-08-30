import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';
import { ApiError } from '../models/api-error.model';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status !== 401) {
        const apiError = error.error as ApiError | undefined;
        const message = apiError?.title ?? 'Something went wrong. Please try again.';
        snackBar.open(message, 'Dismiss', { duration: 5000 });
      }
      return throwError(() => error);
    })
  );
};
