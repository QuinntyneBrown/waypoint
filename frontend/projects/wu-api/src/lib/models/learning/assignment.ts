export interface AssignmentRubricItem {
  id: string;
  description: string;
  points: number;
}

// Traces to: L2-LEARN-006
export interface AssignmentBrief {
  id: string;
  title: string;
  taskDescription: string;
  dueDate: string | null;
  allowedSubmissionTypes: Array<'text' | 'photo' | 'link' | 'file'>;
  rubric: AssignmentRubricItem[];
  totalPoints: number;
  maxUploadBytes: number;
  existingSubmissionId: string | null;
  existingSubmissionStatus: string | null;
}

export interface AssignmentUpload {
  id: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  url: string;
}

export interface SubmitAssignmentInput {
  bodyText: string | null;
  photoUploadId: string | null;
  photoUrl: string | null;
  linkUrl: string | null;
  isCodingProject: boolean;
  youthNote: string | null;
  attest: boolean;
}

export interface SubmissionResult {
  id: string;
}

export interface SubmissionTimelineItem {
  type: string;
  label: string;
  at: string | null;
  current: boolean;
}

export interface SubmissionRubricScore {
  criterionId: string;
  description: string;
  possiblePoints: number;
  awardedPoints: number | null;
}

export interface SubmissionBadge {
  id: string;
  name: string;
  icon: string;
  description: string | null;
  awardedAt: string;
}

// Traces to: L2-LEARN-007
export interface YouthSubmissionStatus {
  id: string;
  assignmentId: string;
  assignmentTitle: string;
  status: string;
  isCodingProject: boolean;
  bodyText: string | null;
  youthNote: string | null;
  photoUrl: string | null;
  linkUrl: string | null;
  mentorComment: string | null;
  totalAwarded: number | null;
  totalPossible: number;
  submittedAt: string;
  reviewedAt: string | null;
  timeline: SubmissionTimelineItem[];
  rubric: SubmissionRubricScore[];
  earnedBadge: SubmissionBadge | null;
}
