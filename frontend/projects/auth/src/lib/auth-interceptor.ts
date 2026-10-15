import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthStore } from './auth-store';
import { SessionStore } from './session-store';

/** Endpoints that must never carry a bearer token (unauthenticated by design). */
const NO_BEARER = /\/api\/auth\/(sign-in|register|refresh)(\/|$|\?)/;
/** Endpoints excluded from refresh-on-401 (avoids refresh loops on the auth surface). */
const NO_RETRY = /\/api\/auth\//;

/**
 * Attaches `Authorization: Bearer` and drives a single-flight refresh on 401.
 * Concurrent 401'd requests await one shared refresh, then retry once; a failed
 * refresh (or family revocation) forces teardown to `/session-expired`.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const store = inject(SessionStore);
  const auth = inject(AuthStore);

  const token = store.accessToken();
  const authReq =
    token && !NO_BEARER.test(req.url)
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

  return next(authReq).pipe(
    catchError((err: unknown) => {
      const status = (err as { status?: number })?.status;
      if (status === 401 && !NO_RETRY.test(req.url) && store.accessToken()) {
        return auth.refreshToken().pipe(
          switchMap((result) =>
            next(authReq.clone({ setHeaders: { Authorization: `Bearer ${result.accessToken}` } })),
          ),
          catchError((refreshErr: unknown) => {
            auth.forceExpire();
            return throwError(() => refreshErr);
          }),
        );
      }
      return throwError(() => err);
    }),
  );
};
