export interface CurriculumTrackOption {
  trackId: string;
  name: string;
  description: string | null;
  selected: boolean;
}

export interface CurriculumItem {
  itemId: string;
  type: 'lesson' | 'coding' | 'quiz';
  title: string;
  order: number;
  status: string;
  locked: boolean;
  unlockCondition: string | null;
  estimatedMinutes: number | null;
}

export interface CurriculumModule {
  moduleId: string;
  name: string;
  order: number;
  status: string;
  completionPercent: number;
  locked: boolean;
  unlockCondition: string | null;
  description: string | null;
  scriptureAnchor: string | null;
  items: CurriculumItem[];
}

// Traces to: L2-LEARN-001
export interface YouthCurriculum {
  selectedTrackId: string;
  tracks: CurriculumTrackOption[];
  modules: CurriculumModule[];
}

export interface LessonMedia {
  mediaId: string;
  mediaType: string;
  url: string;
}

export interface LessonReadingState {
  textSize: 'small' | 'medium' | 'large';
  bookmarked: boolean;
  position: number;
}

// Traces to: L2-LEARN-002
export interface YouthLesson {
  lessonId: string;
  moduleId: string;
  moduleName: string;
  title: string;
  scriptureReference: string | null;
  learningObjectives: string | null;
  teachingBody: string | null;
  reflectionPrompts: string | null;
  estimatedMinutes: number | null;
  stepNumber: number;
  totalSteps: number;
  status: string;
  media: LessonMedia[];
  readingState: LessonReadingState;
}

export interface YouthReflectionEntry {
  id: string;
  text: string | null;
  mood: string | null;
  secondaryResponses: Record<string, string>;
  shareWithMentor: boolean;
  isDraft: boolean;
  updatedAt: string;
}

// Traces to: L2-LEARN-003
export interface ReflectionEditor {
  lessonId: string;
  lessonTitle: string;
  prompt: string | null;
  reflection: YouthReflectionEntry | null;
}

export interface SaveReflectionInput {
  lessonId: string;
  text: string | null;
  mood: string | null;
  secondaryResponses: Record<string, string>;
  shareWithMentor: boolean;
  isDraft: boolean;
}

export interface SaveReflectionResult {
  id: string;
}
