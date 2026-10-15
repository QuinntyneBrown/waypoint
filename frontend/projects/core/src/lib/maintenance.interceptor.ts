import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

/**
 * Routes to the maintenance state when the API reports 503 (maintenance mode,
 * L2-PLATFORM-009). Presentational only — the server is the authority.
 */
export const maintenanceInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  return next(req).pipe(
    catchError((err: unknown) => {
      if ((err as { status?: number })?.status === 503) {
        void router.navigateByUrl('/maintenance');
      }
      return throwError(() => err);
    }),
  );
};
