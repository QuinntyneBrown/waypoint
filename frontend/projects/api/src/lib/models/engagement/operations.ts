// Traces to: L2-ENGAGE-001..004, L2-NFR-004/006

export interface SessionSchedulingOptions {
  cohorts: SessionCohortOption[];
  mentors: SessionMentorOption[];
}

export interface SessionCohortOption {
  cohortId: string;
  name: string;
  termStart: string;
  termEnd: string;
}

export interface SessionMentorOption {
  mentorId: string;
  name: string;
}

export interface CalendarSession {
  id: string;
  cohortId: string;
  mentorId: string | null;
  title: string;
  startsAt: string;
  durationMinutes: number | null;
  location: string | null;
  cohortName: string;
  mentorName: string;
  attendanceTaken: boolean;
  rosterCount: number;
}

export interface CreateSessionRequest {
  cohortId: string;
  mentorId: string;
  title: string;
  startsAt: string;
  durationMinutes: number | null;
  location: string | null;
  repeatsWeekly: boolean;
}

export interface CreateSessionResult {
  firstSessionId: string;
  generatedCount: number;
}

export type AttendanceStatus = 'Present' | 'Late' | 'Excused' | 'Absent';

export interface AttendanceMarkInput {
  youthId: string;
  status: AttendanceStatus;
  note: string | null;
  expectedVersion: string | null;
}

export interface AttendanceWrite {
  sessionId: string;
  marks: AttendanceMarkInput[];
}

export interface AttendanceTally {
  present: number;
  late: number;
  excused: number;
  absent: number;
  marked: number;
  total: number;
}

export interface StaffSessionAttendance {
  sessionId: string;
  title: string;
  startsAt: string;
  durationMinutes: number | null;
  location: string | null;
  cohortId: string;
  cohortName: string;
  roster: StaffAttendanceRosterEntry[];
}

export interface StaffAttendanceRosterEntry {
  youthUserId: string;
  name: string;
  mark: AttendanceStatus | null;
  note: string | null;
  version: string;
}

export interface SessionAttendanceSummary {
  sessionId: string;
  title: string;
  startsAt: string;
  present: number;
  marked: number;
  rate: number;
  status: 'saved' | 'needs review' | 'low turnout';
  cohortId: string;
  cohortName: string;
  rosterTotal: number;
  location: string | null;
}

export interface YouthAttendanceSummary {
  youthUserId: string;
  attended: number;
  late: number;
  excused: number;
  absent: number;
  total: number;
  rate: number;
  belowThreshold: boolean;
  displayName: string;
  trend: 'improving' | 'declining' | 'steady' | 'no data';
}

export interface AttendanceHistoryFilter {
  cohortId?: string;
  from?: string;
  to?: string;
}
