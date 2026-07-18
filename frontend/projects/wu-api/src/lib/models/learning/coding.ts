export interface CodingLessonStep {
  id: string;
  order: number;
  title: string;
  instructions: string;
  codeSample: string;
  checklistLabel: string;
}

export interface CodingLessonBadge {
  id: string;
  name: string;
  icon: string;
  completedCriteria: number;
  totalCriteria: number;
  earned: boolean;
}

// Traces to: L2-LEARN-008
export interface CodingLesson {
  lessonId: string;
  assignmentId: string;
  title: string;
  introduction: string | null;
  code: string;
  steps: CodingLessonStep[];
  checkedStepIds: string[];
  completionPercent: number;
  completed: boolean;
  badge: CodingLessonBadge | null;
  existingSubmissionId: string | null;
  existingSubmissionStatus: string | null;
}

export interface CodingProgressResult {
  completedCriteria: number;
  totalCriteria: number;
  completionPercent: number;
  completed: boolean;
  badgeEarned: boolean;
}

export interface SaveCodingProgressInput {
  code: string;
  checkedStepIds: string[];
}

export interface SubmitCodingProjectInput {
  code: string;
  mentorNote: string | null;
  attest: boolean;
}
