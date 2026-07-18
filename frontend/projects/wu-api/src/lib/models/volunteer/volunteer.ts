/** Volunteer portal models (L2-VOLUNTEER-001/002). Names-only / no youth records. */

// Traces to: L2-VOLUNTEER-001 (AC1 today's session logistics + roster count, no youth data)
export interface VolunteerTodayView {
  sessionId: string;
  title: string;
  startsAt: string;
  durationMinutes: number | null;
  location: string | null;
  rosterCount: number;
}

// Traces to: L2-VOLUNTEER-002 (AC1 upcoming assigned sessions + derived status)
export interface VolunteerScheduleItem {
  sessionId: string;
  title: string;
  startsAt: string;
  durationMinutes: number | null;
  location: string | null;
  status: string;
}
