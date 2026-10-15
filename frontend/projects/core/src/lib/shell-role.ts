import { WRole } from '@waypoint/components';

const MAP: Record<string, WRole> = {
  Youth: 'youth',
  Mentor: 'mentor',
  ProgramAdministrator: 'admin',
  Guardian: 'guardian',
  Volunteer: 'volunteer',
};

/** Map a backend role string to the shell's WRole. */
export function shellRole(role: string | null | undefined): WRole {
  return (role && MAP[role]) || 'youth';
}
