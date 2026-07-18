import { WuRole } from '@wu/ui';

const MAP: Record<string, WuRole> = {
  Youth: 'youth',
  Mentor: 'mentor',
  ProgramAdministrator: 'admin',
  Guardian: 'guardian',
  Volunteer: 'volunteer',
};

/** Map a backend role string to the shell's WuRole. */
export function shellRole(role: string | null | undefined): WuRole {
  return (role && MAP[role]) || 'youth';
}
