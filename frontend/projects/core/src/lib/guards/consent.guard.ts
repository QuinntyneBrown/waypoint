import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { ConsentService } from '@waypoint/api';
import { catchError, map, of } from 'rxjs';

/**
 * Youth participation gate. Re-checks `GET /api/consent/status` on every youth
 * navigation (L2-CONSENT-002 AC5, client mirror) so access unblocks the moment
 * a guardian grants consent. Presentational only — the API is the authority and
 * redacts protected data regardless — so a transient status error fails open.
 */
export const consentGuard: CanActivateFn = () => {
  const consent = inject(ConsentService);
  const router = inject(Router);
  return consent.status().pipe(
    map((status) => status.participationAllowed || router.parseUrl('/consent-required')),
    catchError(() => of(true)),
  );
};
