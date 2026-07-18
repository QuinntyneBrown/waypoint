import { DOCUMENT } from '@angular/common';
import { inject, InjectionToken } from '@angular/core';

export interface WordUpDeploymentUrls {
  /** Origin or path prefix where the authenticated portal is deployed. */
  portalBaseUrl: string;
  /** Origin or path prefix where the anonymous public site is deployed. */
  publicSiteBaseUrl: string;
}

interface RuntimeConfigWindow extends Window {
  __WORD_UP_CONFIG__?: Partial<WordUpDeploymentUrls>;
}

function withoutTrailingSlash(value: string): string {
  return value === '/' ? '' : value.replace(/\/+$/, '');
}

function isLocalDevelopment(location: Location): boolean {
  return location.hostname === 'localhost' || location.hostname === '127.0.0.1';
}

/**
 * Cross-application deployment boundary used by both Angular applications.
 *
 * Deployments can replace `/runtime-config.js` in either app and set
 * `window.__WORD_UP_CONFIG__` without rebuilding bundles. The same-origin defaults expect the
 * portal below `/portal` and the public site at `/`; local development maps ports 4200/4201.
 */
export const WORD_UP_DEPLOYMENT_URLS = new InjectionToken<WordUpDeploymentUrls>(
  'wu.deployment.urls',
  {
    providedIn: 'root',
    factory: () => {
      const window = inject(DOCUMENT).defaultView as RuntimeConfigWindow | null;
      const location = window?.location;
      const runtime = window?.__WORD_UP_CONFIG__;
      const local = location ? isLocalDevelopment(location) : false;

      const localPortal =
        local && location?.port === '4201'
          ? `${location.protocol}//${location.hostname}:4200`
          : '/portal';
      const localPublicSite =
        local && location?.port === '4200' ? `${location.protocol}//${location.hostname}:4201` : '';

      return {
        portalBaseUrl: withoutTrailingSlash(runtime?.portalBaseUrl ?? localPortal),
        publicSiteBaseUrl: withoutTrailingSlash(runtime?.publicSiteBaseUrl ?? localPublicSite),
      };
    },
  },
);

/** Join a deployment base with an app route without creating a client-router navigation. */
export function deploymentUrl(baseUrl: string, path: `/${string}`): string {
  return `${withoutTrailingSlash(baseUrl)}${path}`;
}
