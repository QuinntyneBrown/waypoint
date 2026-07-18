export interface CurriculumTrackOption {
  trackId: string;
  name: string;
  description: string | null;
  selected: boolean;
}

export interface CurriculumItem {
  itemId: string;
  type: 'lesson' | 'quiz';
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
