import { InjectionToken } from '@angular/core';

/**
 * Base URL prepended to every `/api/*` request made by wu-api services.
 *
 * Defaults to `''` (empty string) so requests are same-origin and relative:
 * in dev the Angular dev-server proxy forwards `/api` to the backend, and in
 * production the SPA is served from the same origin as the API. Tests may
 * override it to point at an absolute API base.
 */
export const API_BASE_URL = new InjectionToken<string>('wu.api.baseUrl', {
  providedIn: 'root',
  factory: () => '',
});
