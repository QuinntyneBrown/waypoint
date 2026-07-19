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

export interface SubmissionReviewView {
  id: string;
  youthId: string;
  youthName: string;
  assignmentTitle: string;
  taskDescription: string;
  status: string;
  isCodingProject: boolean;
  bodyText: string | null;
  photoUrl: string | null;
  linkUrl: string | null;
  youthNote: string | null;
  submittedAt: string;
  mentorComment: string | null;
  totalAwarded: number | null;
  totalPossible: number;
  guardianNotified: boolean;
  rubric: SubmissionReviewCriterion[];
}

export interface SubmissionReviewCriterion {
  criterionId: string;
  description: string;
  possiblePoints: number;
  awardedPoints: number | null;
}

export interface GradeSubmissionRequest {
  decision: 'approve' | 'request-revision' | 'save-draft';
  rubricScoresJson: string | null;
  totalAwarded: number | null;
  comment: string | null;
  guardianNotified: boolean;
}

export interface QuizAssessmentReview {
  attemptId: string;
  youthId: string;
  youthName: string;
  quizId: string;
  quizTitle: string;
  attemptNumber: number;
  autoScore: number;
  approvedScore: number | null;
  passMark: number;
  autoPassed: boolean;
  approvedPassed: boolean | null;
  status: 'Pending' | 'Approved';
  submittedAt: string;
  approvedAt: string | null;
  approvedByName: string | null;
  questions: QuizAssessmentQuestion[];
}

export interface QuizAssessmentQuestion {
  questionId: string;
  text: string;
  options: string[];
  selectedIndexes: number[];
  correctIndexes: number[];
  isCorrect: boolean;
}

export type CompetencyLevel = 'NotStarted' | 'Developing' | 'Proficient' | 'Mastered';

export interface MentorCompetencySummary {
  youthId: string;
  youthName: string;
  currentStageName: string | null;
  currentStageProgress: number;
  items: MentorCompetencyItem[];
  masteredOrProficient: number;
  total: number;
}

export interface MentorCompetencyItem {
  competencyId: string;
  name: string;
  stageName: string;
  level: CompetencyLevel;
  notes: string | null;
  assessedByName: string | null;
  assessedAt: string | null;
}

export interface SetCompetencyLevelRequest {
  competencyId: string;
  level: CompetencyLevel;
  notes: string | null;
}

export interface MentorCheckIn {
  id: string;
  youthUserId: string;
  youthName: string;
  date: string;
  type: '1:1' | 'call' | 'session';
  engagement: string | null;
  goalsDiscussed: string | null;
  prayerRequests: string | null;
  actionItems: string | null;
  visibility: 'Private' | 'SharedWithMentor';
  nextFollowUp: string | null;
  guardianInformed: boolean;
  createdAt: string;
}

export interface LogCheckInRequest {
  youthId: string;
  date: string;
  type: '1:1' | 'call' | 'session';
  engagement: string | null;
  goalsDiscussed: string | null;
  prayerRequests: string | null;
  actionItems: string | null;
  nextFollowUp: string | null;
  shareWithYouthAndGuardian: boolean;
  guardianInformed: boolean;
}

export interface AtRiskYouth {
  flagId: string;
  youthId: string;
  displayName: string;
  reasons: string[];
  flaggedAt: string;
  flaggedByName: string;
  automatic: boolean;
}

export type FlagResolutionReason =
  'Reconnected' | 'HandedToDirector' | 'LeftProgram' | 'FlaggedInError';

export type AssignWorkItemType = 'Lesson' | 'Module' | 'Project';
export type ReminderCadence = '1-day-before' | '2-days-before';

export interface AssignWorkOptions {
  items: AssignableWorkItem[];
  youth: AssignableYouth[];
  cohorts: AssignableCohort[];
}

export interface AssignableWorkItem {
  itemId: string;
  itemType: AssignWorkItemType;
  title: string;
  moduleName: string;
}

export interface AssignableYouth {
  youthId: string;
  displayName: string;
  cohortId: string | null;
  cohortName: string | null;
  atRisk: boolean;
}

export interface AssignableCohort {
  cohortId: string;
  name: string;
  youthCount: number;
}

export interface AssignWorkRequest {
  itemType: AssignWorkItemType;
  itemId: string;
  youthIds: string[] | null;
  cohortId: string | null;
  dueDate: string | null;
  reminderCadence: ReminderCadence | null;
  message: string | null;
}
