import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  // These routes render database-backed public content and therefore cannot be frozen at build
  // time. Rendering per request preserves SEO while keeping GET /api/public/content authoritative.
  { path: '', renderMode: RenderMode.Server },
  { path: 'programs', renderMode: RenderMode.Server },
  { path: 'about', renderMode: RenderMode.Server },
  {
    path: '**',
    renderMode: RenderMode.Prerender,
  },
];
