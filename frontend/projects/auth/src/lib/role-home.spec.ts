import { roleHome } from './role-home';

describe('roleHome', () => {
  it('maps every canonical role to its home surface', () => {
    expect(roleHome('Youth')).toBe('/youth/home');
    expect(roleHome('Mentor')).toBe('/mentor/dashboard');
    expect(roleHome('ProgramAdministrator')).toBe('/admin/dashboard');
    expect(roleHome('Guardian')).toBe('/guardian/home');
    expect(roleHome('Volunteer')).toBe('/volunteer/home');
  });

  it('falls back to youth home for unknown/empty roles', () => {
    expect(roleHome(null)).toBe('/youth/home');
    expect(roleHome(undefined)).toBe('/youth/home');
    expect(roleHome('Nonsense')).toBe('/youth/home');
  });
});
