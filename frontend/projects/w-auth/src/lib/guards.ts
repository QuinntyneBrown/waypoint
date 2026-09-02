import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { roleHome } from './role-home';
import { SessionStore } from './session-store';

/**
 * Requires an authenticated session; otherwise routes to sign-in with a
 * returnUrl. Presentational only — the API remains the access authority.
 */
export const authGuard: CanActivateFn = (_route, state): boolean | UrlTree => {
  const store = inject(SessionStore);
  const router = inject(Router);
  if (store.isAuthenticated()) return true;
  // A stored session that failed to restore (expired/revoked) → session-expired
  // state (drafts preserved); a first-time/unauthenticated visitor → sign-in.
  if (store.expiredOnBoot()) return router.parseUrl('/session-expired');
  return router.createUrlTree(['/auth/sign-in'], {
    queryParams: state.url && state.url !== '/' ? { returnUrl: state.url } : undefined,
  });
};

/** Keeps signed-in users out of the auth screens, sending them to their role home. */
export const guestGuard: CanActivateFn = (): boolean | UrlTree => {
  const store = inject(SessionStore);
  const router = inject(Router);
  if (!store.isAuthenticated()) return true;
  return router.parseUrl(roleHome(store.role()));
};
