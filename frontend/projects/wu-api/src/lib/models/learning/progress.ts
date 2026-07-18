export interface YouthQuizHistoryItem {
  attemptId: string;
  quizId: string;
  quizTitle: string;
  moduleName: string;
  score: number;
  passed: boolean;
  approvedAt: string;
}

export interface YouthAttendanceHistoryItem {
  sessionId: string;
  sessionTitle: string;
  startsAt: string;
  status: 'Present' | 'Late' | 'Excused' | 'Absent';
}

export interface YouthCompetencyProgress {
  competencyId: string;
  name: string;
  level: 'NotStarted' | 'Developing' | 'Proficient' | 'Mastered';
  percent: number;
  assessedAt: string;
}

export interface YouthSharedCheckIn {
  checkInId: string;
  date: string;
  type: string;
  engagement: string | null;
  goalsDiscussed: string | null;
  prayerRequests: string | null;
  actionItems: string | null;
  nextFollowUp: string | null;
}

// Traces to: L2-LEARN-010
export interface YouthProgress {
  overallPercent: number;
  quizAverage: number;
  lessonsCompleted: number;
  lessonsTotal: number;
  attendanceRate: number;
  quizHistory: YouthQuizHistoryItem[];
  attendanceHistory: YouthAttendanceHistoryItem[];
  competencies: YouthCompetencyProgress[];
  sharedCheckIns: YouthSharedCheckIn[];
}
