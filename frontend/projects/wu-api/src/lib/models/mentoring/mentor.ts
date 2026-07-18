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
