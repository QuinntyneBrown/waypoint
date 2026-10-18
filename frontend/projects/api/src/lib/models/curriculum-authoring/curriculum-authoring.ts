export type PublishStatus = 'Draft' | 'Published';
export type BadgeCriteriaKind = 'Auto' | 'Manual';
export type QuestionType = 'MultipleChoice' | 'TrueFalse' | 'MultipleAnswer' | 'ShortAnswer';

export interface IdResult {
  id: string;
}

export interface TrackSummary {
  id: string;
  name: string;
  key: string;
  status: PublishStatus;
  enabled: boolean;
}

export interface TrackWorkspace extends TrackSummary {
  description: string | null;
  assignedCohortNames: string[];
  modules: ModuleWorkspace[];
}

export interface ModuleWorkspace {
  id: string;
  name: string;
  description: string | null;
  order: number;
  prerequisiteModuleId: string | null;
  scriptureAnchor: string | null;
  status: PublishStatus;
  lessons: LessonWorkspace[];
  quizzes: QuizWorkspace[];
  assignments: AssignmentWorkspace[];
}

export interface LessonWorkspace {
  id: string;
  title: string;
  order: number;
  status: PublishStatus;
  scriptureReference: string | null;
  estimatedMinutes: number | null;
  learningObjectives: string | null;
  teachingBody: string | null;
  reflectionPrompts: string | null;
  leaderGuideNotes: string | null;
  media: LessonMediaWorkspace[];
  quizzes: QuizWorkspace[];
  assignments: AssignmentWorkspace[];
}

export type VideoStatus = 'None' | 'Queued' | 'Processing' | 'Ready' | 'Failed';

export interface LessonMediaWorkspace {
  id: string;
  mediaType: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  url: string;
  /** 'None' for documents and for videos stored before streaming existed. */
  videoStatus: VideoStatus;
  progressPercent: number;
  durationSeconds: number | null;
  failureCode: string | null;
}

export interface VideoUploadSession {
  uploadId: string;
  chunkSizeBytes: number;
  chunkCount: number;
}

export interface VideoUploadStatus extends VideoUploadSession {
  receivedChunks: number[];
}

export interface QuizWorkspace {
  id: string;
  moduleId: string | null;
  lessonId: string | null;
  passMark: number;
  attemptsAllowed: number;
  feedbackMode: string;
  shuffle: boolean;
  requiredToCompleteModule: boolean;
  questionCount: number;
}

export interface QuizDetail extends QuizWorkspace {
  questions: QuizQuestionDetail[];
}

export interface QuizQuestionDetail {
  id: string;
  text: string;
  type: QuestionType;
  order: number;
  options: string[];
  correctIndexes: number[];
  correctAnswers: string[];
}

export interface AssignmentWorkspace {
  id: string;
  moduleId: string | null;
  lessonId: string | null;
  title: string;
  taskDescription: string;
  submissionTypes: string[];
  dueDate: string | null;
  gracePeriodDays: number;
  allowLate: boolean;
  requireMentorApproval: boolean;
  totalPoints: number;
  status: PublishStatus;
}

export interface AssignmentDetail extends AssignmentWorkspace {
  rubric: RubricCriterion[];
}

export interface RubricCriterion {
  id: string;
  description: string;
  points: number;
  order: number;
}

export interface SaveTrack {
  name: string;
  key: string;
  description: string | null;
}

export interface SaveModule {
  trackId: string;
  name: string;
  description: string | null;
  prerequisiteModuleId: string | null;
  scriptureAnchor: string | null;
}

export interface SaveLesson {
  moduleId: string;
  title: string;
  scriptureReference: string | null;
  estimatedMinutes: number | null;
  learningObjectives: string | null;
  teachingBody: string | null;
  reflectionPrompts: string | null;
  leaderGuideNotes: string | null;
}

export interface SaveQuiz {
  moduleId: string | null;
  lessonId: string | null;
  passMark: number;
  attemptsAllowed: number;
  feedbackMode: string;
  shuffle: boolean;
  requiredToCompleteModule: boolean;
}

export interface SaveQuestion {
  text: string;
  type: QuestionType;
  options: string[];
  correctIndexes: number[];
  correctAnswers: string[];
}

export interface SaveAssignment {
  moduleId: string | null;
  lessonId: string | null;
  title: string;
  taskDescription: string;
  submissionTypes: string[];
  dueDate: string | null;
  gracePeriodDays: number;
  allowLate: boolean;
  requireMentorApproval: boolean;
  rubric: { description: string; points: number }[];
}

export interface BadgeDefinition {
  id: string;
  name: string;
  icon: string;
  description: string | null;
  criteriaKind: BadgeCriteriaKind;
  autoRule: string | null;
  targetRefId: string | null;
  competencyId: string | null;
  competencyName: string | null;
  awardedCount: number;
}

export interface SaveBadge {
  name: string;
  icon: string;
  description: string | null;
  criteriaKind: BadgeCriteriaKind;
  autoRule: string | null;
  targetRefId: string | null;
  competencyId: string | null;
}

export interface ProgressionStage {
  id: string;
  name: string;
  description: string | null;
  order: number;
  competencies: ProgressionCompetency[];
}

export interface ProgressionCompetency {
  id: string;
  name: string;
  order: number;
}
