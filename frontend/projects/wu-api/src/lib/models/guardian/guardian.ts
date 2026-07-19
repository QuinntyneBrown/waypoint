/** Guardian-safe linked-child aggregate. No learning content or private records. */
export interface GuardianChildSummary {
  childId: string;
  childName: string;
}

export interface GuardianModuleStatus {
  moduleId: string;
  name: string;
  order: number;
  status: string;
  completionPercent: number;
  locked: boolean;
  unlockCondition: string | null;
}

export interface GuardianAttendanceSummary {
  present: number;
  late: number;
  excused: number;
  absent: number;
  total: number;
  rate: number;
}

export interface GuardianAttendanceItem {
  sessionId: string;
  title: string;
  startsAt: string;
  location: string | null;
  status: string;
}

export interface GuardianQuizOutcome {
  attemptId: string;
  quizId: string;
  quizTitle: string;
  moduleName: string;
  score: number;
  passed: boolean;
  approvedAt: string;
}

export interface GuardianCompetencyOutcome {
  competencyId: string;
  name: string;
  level: string;
  percent: number;
  assessedAt: string;
}

export interface GuardianBadge {
  badgeId: string;
  name: string;
  icon: string;
  description: string | null;
}

export interface GuardianUpcomingSession {
  sessionId: string;
  title: string;
  startsAt: string;
  location: string | null;
}

export interface GuardianSharedCheckIn {
  checkInId: string;
  date: string;
  type: string;
  engagement: string | null;
  goalsDiscussed: string | null;
  prayerRequests: string | null;
  actionItems: string | null;
  nextFollowUp: string | null;
}

// Traces to: L2-GUARDIAN-001
export interface GuardianChildProgress {
  childId: string;
  childName: string;
  overallPercent: number;
  modules: GuardianModuleStatus[];
  attendance: GuardianAttendanceSummary;
  attendanceHistory: GuardianAttendanceItem[];
  quizHistory: GuardianQuizOutcome[];
  competencies: GuardianCompetencyOutcome[];
  earnedBadges: GuardianBadge[];
  lockedBadges: GuardianBadge[];
  upcomingSessions: GuardianUpcomingSession[];
  sharedCheckIns: GuardianSharedCheckIn[];
}

// Traces to: L2-GUARDIAN-002
export interface GuardianAnnouncement {
  id: string;
  title: string;
  message: string;
  sentAt: string | null;
  read: boolean;
}

export interface GuardianContact {
  displayName: string;
  email: string;
  relationship: string | null;
  phone: string | null;
  emergencyContactName: string | null;
  emergencyContactPhone: string | null;
}

export interface GuardianConsentRecord {
  consentId: string;
  youthUserId: string;
  guardianName: string;
  relationship: string;
  participationConsent: boolean;
  mediaConsent: boolean;
  mediaAllPhotosVideo: boolean;
  mediaGroupOnly: boolean;
  mediaFirstNameCaptions: boolean;
  method: string;
  referenceId: string;
  term: string | null;
  signedAt: string;
  validFrom: string;
  validUntil: string;
  withdrawnAt: string | null;
}

export interface GuardianContactUpdate {
  displayName: string;
  email: string;
  relationship: string | null;
  phone: string | null;
  emergencyContactName: string | null;
  emergencyContactPhone: string | null;
}

export interface GuardianMediaConsentUpdate {
  mediaConsent: boolean;
  mediaAllPhotosVideo: boolean;
  mediaGroupOnly: boolean;
  mediaFirstNameCaptions: boolean;
}
