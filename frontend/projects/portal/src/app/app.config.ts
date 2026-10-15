import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { authInterceptor, provideWuAuth } from '@waypoint/auth';
import { routes } from './app.routes';
import { apiStateInterceptor, maintenanceInterceptor } from '@waypoint/core';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(
      withInterceptors([authInterceptor, apiStateInterceptor, maintenanceInterceptor]),
    ),
    provideWuAuth(),
  ],
};
