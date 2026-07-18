import { mergeApplicationConfig, ApplicationConfig } from '@angular/core';
import { provideServerRendering, withRoutes } from '@angular/ssr';
import { API_BASE_URL } from '@wu/api';
import { appConfig } from './app.config';
import { serverRoutes } from './app.routes.server';

const serverConfig: ApplicationConfig = {
  providers: [
    provideServerRendering(withRoutes(serverRoutes)),
    // At deployment the SSR process can call the API over its private service address while
    // browser requests stay same-origin. Angular's dev-server proxy remains the empty default.
    {
      provide: API_BASE_URL,
      useFactory: () => process.env['WORD_UP_API_BASE_URL'] ?? '',
    },
  ],
};

export const config = mergeApplicationConfig(appConfig, serverConfig);
