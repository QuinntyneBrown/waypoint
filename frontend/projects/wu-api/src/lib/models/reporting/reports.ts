// Traces to: L2-REPORT-001
export interface ReportFilter {
  from?: string;
  to?: string;
  cohortId?: string;
  trackId?: string;
  mentorId?: string;
}

export interface ReportOption {
  id: string;
  name: string;
}

export interface ReportOptions {
  cohorts: ReportOption[];
  tracks: ReportOption[];
  mentors: ReportOption[];
  youth: ReportOption[];
}

export interface AttendanceBreakdown {
  present: number;
  late: number;
  excused: number;
  absent: number;
  total: number;
  rate: number;
}

export interface ReportKpis {
  activeYouth: number;
  cohorts: number;
  avgCompletion: number;
  attendanceRate: number;
  retentionRate: number;
  activeMentors: number;
}

export interface CohortMetric {
  cohortId: string;
  cohortName: string;
  enrolment: number;
  completionRate: number;
  attendanceRate: number;
  retentionRate: number;
  atRisk: number;
}

export interface StaffActivityReportRow {
  mentorId: string;
  mentorName: string;
  email: string;
  caseload: number;
  checkIns: number;
  reviews: number;
  targetMinimum: number;
  targetMaximum: number;
  loadBalance: string;
  overloaded: boolean;
}

export interface ReportsHub {
  generatedAt: string;
  recordCount: number;
  kpis: ReportKpis;
  attendance: AttendanceBreakdown;
  cohorts: CohortMetric[];
  staff: StaffActivityReportRow[];
}

export interface ModuleReport {
  moduleId: string;
  name: string;
  completedLessons: number;
  totalLessons: number;
  completionRate: number;
}

export interface QuizScoreReport {
  quizId: string;
  title: string;
  score: number;
  submittedAt: string | null;
}

export interface CompetencyReport {
  competencyId: string;
  name: string;
  level: string;
  assessedAt: string;
}

export interface MentorNotesSummary {
  checkIns: number;
  latestAt: string | null;
  latestEngagement: string | null;
}

export interface BadgeReport {
  badgeId: string;
  name: string;
  awardedAt: string;
}

export interface YouthReport {
  youthId: string;
  youthName: string;
  cohortName: string | null;
  mentorName: string | null;
  lessonsCompleted: number;
  lessonsTotal: number;
  completionRate: number;
  avgQuizScore: number;
  attendancePresent: number;
  attendanceTotal: number;
  badges: number;
  attendance: AttendanceBreakdown;
  modules: ModuleReport[];
  quizScores: QuizScoreReport[];
  competencies: CompetencyReport[];
  mentorNotes: MentorNotesSummary;
  badgeAwards: BadgeReport[];
}

export interface CohortYouthSummary {
  youthId: string;
  youthName: string;
  completionRate: number;
  attendanceRate: number;
  status: string;
}

export interface CohortComparison {
  label: string;
  completionRate: number;
  attendanceRate: number;
  retentionRate: number;
}

export interface CohortReport {
  cohortId: string;
  cohortName: string;
  enrolment: number;
  avgCompletion: number;
  attendanceRate: number;
  retentionRate: number;
  statusActive: number;
  statusAtRisk: number;
  modules: ModuleReport[];
  youth: CohortYouthSummary[];
  comparisons: CohortComparison[];
}
