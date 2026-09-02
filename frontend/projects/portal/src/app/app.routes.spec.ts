import { Route } from '@angular/router';
import { roleHome } from '@w/auth';
import { routes } from './app.routes';

function route(path: string): Route {
  const found = routes.find((candidate) => candidate.path === path);
  if (!found) throw new Error(`Missing top-level route: ${path}`);
  return found;
}

function child(parent: string, path: string): Route {
  const found = route(parent).children?.find((candidate) => candidate.path === path);
  if (!found) throw new Error(`Missing route: /${parent}/${path}`);
  return found;
}

describe('portal routes', () => {
  it('provides a real default and role home for every authenticated role', () => {
    expect(roleHome('Youth')).toBe('/youth/home');
    expect(roleHome('Mentor')).toBe('/mentor/dashboard');
    expect(roleHome('ProgramAdministrator')).toBe('/admin/dashboard');
    expect(roleHome('Guardian')).toBe('/guardian/home');
    expect(roleHome('Volunteer')).toBe('/volunteer/home');

    expect(child('youth', '').redirectTo).toBe('home');
    expect(child('mentor', '').redirectTo).toBe('dashboard');
    expect(child('admin', '').redirectTo).toBe('dashboard');
    expect(child('guardian', '').redirectTo).toBe('home');
    expect(child('volunteer', '').redirectTo).toBe('home');
  });

  it('wires every formerly dead staff navigation destination', () => {
    expect(child('admin', 'dashboard').loadComponent).toBeTypeOf('function');
    expect(child('admin', 'invitations').loadComponent).toBeTypeOf('function');
    expect(child('admin', 'users-roles').loadComponent).toBeTypeOf('function');
    expect(child('admin', 'privacy').loadComponent).toBeTypeOf('function');
    expect(child('mentor', 'invitations').loadComponent).toBeTypeOf('function');
  });

  it('repairs base attendance and notification links without exposing a missing page', () => {
    expect(child('volunteer', 'attendance').redirectTo).toBe('home');
    expect(child('volunteer', 'notifications').redirectTo).toBe('home');
    expect(child('guardian', 'notifications').redirectTo).toBe('announcements');
    expect(child('admin', 'notifications').redirectTo).toBe('announcements');
  });

  it('exposes invitation acceptance, help, and privacy settings', () => {
    expect(route('invite').loadComponent).toBeTypeOf('function');
    expect(child('auth', 'invite').loadComponent).toBeTypeOf('function');
    expect(route('help').loadComponent).toBeTypeOf('function');
    expect(child('settings', 'privacy').loadComponent).toBeTypeOf('function');
  });
});
