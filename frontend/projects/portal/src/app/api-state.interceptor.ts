import { DOCUMENT } from '@angular/common';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { FormDraftService } from './form-draft.service';

const stateRoute: Record<number, string> = {
  403: '/forbidden',
  404: '/not-found',
  500: '/server-error',
};

/** Map API authorization/resource/server failures to the shared product states. */
export const apiStateInterceptor: HttpInterceptorFn = (request, next) => {
  const router = inject(Router);
  const drafts = inject(FormDraftService);
  const document = inject(DOCUMENT);

  return next(request).pipe(
    catchError((error: unknown) => {
      const status = (error as { status?: number })?.status;
      const state =
        error instanceof HttpErrorResponse ? error.headers.get('X-Waypoint-State') : null;
      if (state === 'media-consent-required') {
        // The active feature owns this recoverable, action-specific consent message.
        return throwError(() => error);
      }
      if (state === 'consent-required' && request.url.includes('/api/')) {
        drafts.captureCurrentForm();
        const window = document.defaultView;
        if (window) {
          window.location.assign('/consent-required');
        } else {
          void router.navigateByUrl('/consent-required');
        }
        return throwError(() => error);
      }
      const route = status === undefined ? undefined : stateRoute[status];
      if (route && request.url.includes('/api/')) {
        if (status === 500) drafts.captureCurrentForm();
        void router.navigateByUrl(route);
      }
      return throwError(() => error);
    }),
  );
};
