export interface YouthHomeLearning {
  itemId: string;
  type: 'lesson' | 'coding' | 'quiz';
  title: string;
  moduleName: string;
  status: string;
  estimatedMinutes: number | null;
  targetPath: string;
}

export interface YouthHomeSession {
  sessionId: string;
  title: string;
  startsAt: string;
  durationMinutes: number | null;
  location: string | null;
  rsvpResponse: 'Going' | 'Maybe' | 'CantAttend' | null;
}

export interface YouthHomeAssignedWork {
  assignmentId: string;
  itemId: string;
  itemType: string;
  title: string;
  dueDate: string | null;
  status: string;
  overdue: boolean;
  message: string | null;
  targetPath: string;
}

// Traces to: L2-LEARN-001..010, L2-MENTOR-008, L2-ENGAGE-005
export interface YouthHomeView {
  placed: boolean;
  cohortName: string | null;
  mentorName: string | null;
  completedItems: number;
  totalItems: number;
  overallPercent: number;
  continueLearning: YouthHomeLearning | null;
  nextSession: YouthHomeSession | null;
  assignedWork: YouthHomeAssignedWork[];
}
