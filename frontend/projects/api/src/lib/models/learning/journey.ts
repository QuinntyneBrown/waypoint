import { CurriculumModule } from './curriculum';

export interface JourneyEarnedBadge {
  badgeId: string;
  name: string;
  icon: string;
  criteria: string;
  awardedAt: string;
}

export interface JourneyLockedBadge {
  badgeId: string;
  name: string;
  icon: string;
  criteria: string;
  completedCriteria: number;
  totalCriteria: number;
  progressLabel: string;
}

export interface JourneyMilestone {
  type: string;
  title: string;
  detail: string;
  occurredAt: string | null;
  current: boolean;
}

// Traces to: L2-LEARN-009
export interface YouthJourney {
  modules: CurriculumModule[];
  earned: JourneyEarnedBadge[];
  locked: JourneyLockedBadge[];
  milestones: JourneyMilestone[];
}
