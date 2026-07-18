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

// Traces to: L2-VOLUNTEER-001 (names and attendance state only; no youth record data)
export interface VolunteerRosterEntry {
  youthUserId: string;
  name: string;
  mark: 'Present' | 'Absent' | null;
  version: string | null;
}

export interface VolunteerAttendanceMark {
  youthId: string;
  status: 'Present' | 'Absent';
  note: null;
  expectedVersion: string | null;
}

export interface VolunteerAttendanceWrite {
  sessionId: string;
  marks: VolunteerAttendanceMark[];
}

export interface AttendanceTally {
  present: number;
  late: number;
  excused: number;
  absent: number;
  marked: number;
  total: number;
}
