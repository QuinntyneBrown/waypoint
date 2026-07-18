import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { authInterceptor, provideWuAuth } from '@wu/auth';
import { routes } from './app.routes';
import { apiStateInterceptor } from './api-state.interceptor';
import { maintenanceInterceptor } from './maintenance.interceptor';

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
