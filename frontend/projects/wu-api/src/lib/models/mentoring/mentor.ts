export interface MentorDashboardView {
  youthCount: number;
  pendingReviews: number;
  overdueReviews: number;
  averageProgress: number;
  atRiskCount: number;
}

export interface MentorRosterYouth {
  id: string;
  displayName: string;
  status: string;
  atRisk: boolean;
  new: boolean;
  progress: number;
  cohortId: string | null;
  cohortName: string | null;
  lastActiveAt: string;
}

export interface MentorReviewPreview {
  reviewId: string;
  youthId: string;
  youthName: string;
  itemType: string;
  itemTitle: string;
  submittedAt: string;
  dueDate: string | null;
  overdue: boolean;
  reviewPath: string;
}

export interface MentorYouthDetail {
  id: string;
  displayName: string;
  email: string;
  pronouns: string | null;
  photoUrl: string | null;
  cohortId: string | null;
  cohortName: string | null;
  guardianUserId: string | null;
  guardian: MentorGuardianContact | null;
  atRisk: boolean;
  atRiskReason: string | null;
  flaggedBy: string | null;
  flaggedByName: string | null;
  flaggedAt: string | null;
  progress: MentorYouthProgress;
  attendance: MentorYouthAttendance[];
  checkIns: MentorYouthCheckIn[];
  submissions: MentorYouthSubmission[];
  competencies: MentorYouthCompetency[];
}

export interface MentorGuardianContact {
  name: string;
  email: string;
  phone: string | null;
  emergencyContactName: string | null;
  emergencyContactPhone: string | null;
}

export interface MentorYouthProgress {
  completedLessons: number;
  totalLessons: number;
  percent: number;
}

export interface MentorYouthAttendance {
  sessionId: string;
  sessionTitle: string;
  startsAt: string;
  status: string;
}

export interface MentorYouthCheckIn {
  id: string;
  date: string;
  type: string;
  visibility: string;
  nextFollowUp: string | null;
}

export interface MentorYouthSubmission {
  id: string;
  assignmentTitle: string;
  status: string;
  submittedAt: string;
}

export interface MentorYouthCompetency {
  competencyId: string;
  name: string;
  level: string;
  notes: string | null;
  assessedAt: string;
}
