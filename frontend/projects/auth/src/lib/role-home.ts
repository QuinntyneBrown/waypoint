/** Home route per backend role string. Presentational routing only — the API is the access authority. */
const ROLE_HOME: Record<string, string> = {
  Youth: '/youth/home',
  Mentor: '/mentor/dashboard',
  ProgramAdministrator: '/admin/dashboard',
  Guardian: '/guardian/home',
  Volunteer: '/volunteer/home',
};

export function roleHome(role: string | null | undefined): string {
  return (role && ROLE_HOME[role]) || '/youth/home';
}
