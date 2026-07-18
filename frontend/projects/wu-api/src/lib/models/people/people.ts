/** Program-administration people-directory models (L2-ADMIN-001). */

// Traces to: L2-ADMIN-001 (AC1 people listed with role/cohort/mentor/status/last-active)
export interface PersonSummary {
  id: string;
  displayName: string;
  email: string;
  role: string;
  status: string;
  cohortId: string | null;
  mentorId: string | null;
  lastActive: string | null;
}

export interface PagedPeople {
  items: PersonSummary[];
  total: number;
  page: number;
  pageSize: number;
}

export interface ListPeopleParams {
  page?: number;
  pageSize?: number;
  role?: string | null;
  cohortId?: string | null;
  search?: string | null;
}

export interface DirectoryCohort {
  id: string;
  name: string;
}

// Traces to: L2-ADMIN-002 (person record and account lifecycle)
export interface PersonDetail {
  id: string;
  displayName: string;
  email: string;
  role: string;
  status: 'Pending' | 'Active' | 'Deactivated' | 'Removed';
  pronouns: string | null;
  photoUrl: string | null;
  onboardingCompleted: boolean;
  createdAt: string;
  lastActive: string | null;
  cohortId: string | null;
  cohortName: string | null;
  mentorId: string | null;
  mentorName: string | null;
  guardianUserId: string | null;
  guardian: PersonGuardianContact | null;
  hasValidConsent: boolean;
  consent: PersonConsentSummary;
  progress: PersonProgressSummary;
  attendance: PersonAttendanceSummary;
  notes: PersonNoteSummary[];
}

export interface PersonGuardianContact {
  name: string;
  email: string;
  relationship: string | null;
  phone: string | null;
  emergencyContactName: string | null;
  emergencyContactPhone: string | null;
}

export interface PersonConsentSummary {
  hasValidParticipationConsent: boolean;
  participationConsent: boolean;
  mediaConsent: boolean;
  signedAt: string | null;
  validUntil: string | null;
  withdrawnAt: string | null;
  guardianName: string | null;
  relationship: string | null;
}

export interface PersonProgressSummary {
  completedLessons: number;
  totalLessons: number;
  percent: number;
}

export interface PersonAttendanceSummary {
  attended: number;
  total: number;
  rate: number;
  items: PersonAttendanceItem[];
}

export interface PersonAttendanceItem {
  sessionId: string;
  sessionTitle: string;
  startsAt: string;
  status: string;
  note: string | null;
}

export interface PersonNoteSummary {
  id: string;
  date: string;
  type: string;
  mentorName: string;
  summary: string;
  nextFollowUp: string | null;
}
