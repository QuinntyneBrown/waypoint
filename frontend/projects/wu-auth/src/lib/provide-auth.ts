import {
  EnvironmentProviders,
  inject,
  makeEnvironmentProviders,
  provideAppInitializer,
} from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AuthStore } from './auth-store';

/**
 * Wire the session boot-restore. Register alongside `provideHttpClient(
 * withInterceptors([authInterceptor]))`. On boot, a stored refresh token
 * re-mints the access token before the first guarded route resolves.
 */
export function provideWuAuth(): EnvironmentProviders {
  return makeEnvironmentProviders([
    provideAppInitializer(() => firstValueFrom(inject(AuthStore).restore())),
  ]);
}
