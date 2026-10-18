import { Role } from '@waypoint/components';

const MAP: Record<string, Role> = {
  Youth: 'youth',
  Mentor: 'mentor',
  ProgramAdministrator: 'admin',
  Guardian: 'guardian',
  Volunteer: 'volunteer',
};

/** Map a backend role string to the shell's Role. */
export function shellRole(role: string | null | undefined): Role {
  return (role && MAP[role]) || 'youth';
}
