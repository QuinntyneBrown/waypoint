export interface QuizBadge {
  id: string;
  name: string;
  icon: string;
  description: string | null;
}

// Traces to: L2-LEARN-004
export interface QuizIntro {
  quizId: string;
  title: string;
  questionCount: number;
  estimatedMinutes: number;
  attemptsAllowed: number;
  passMark: number;
  badgeReward: QuizBadge | null;
  currentAttemptNumber: number;
  attemptsRemaining: number;
  hasInProgressAttempt: boolean;
  attemptsExhausted: boolean;
}

export interface YouthQuizQuestion {
  id: string;
  text: string;
  type: string;
  options: string[];
}

export interface YouthQuizAttempt {
  attemptId: string;
  quizId: string;
  title: string;
  attemptNumber: number;
  questionCount: number;
  estimatedMinutes: number;
  attemptsAllowed: number;
  passMark: number;
  badgeReward: QuizBadge | null;
  currentQuestionIndex: number;
  answersJson: string | null;
  questions: YouthQuizQuestion[];
}

export interface QuizQuestionReview {
  questionId: string;
  text: string;
  options: string[];
  selectedIndexes: number[];
  correctIndexes: number[];
  correct: boolean;
}

// Traces to: L2-LEARN-005
export interface YouthQuizResult {
  attemptId: string;
  quizId: string;
  attemptNumber: number;
  score: number | null;
  passMark: number;
  passed: boolean | null;
  status: 'InProgress' | 'PendingApproval' | 'Approved';
  feedbackMode: string;
  review: QuizQuestionReview[];
  earnedBadge: QuizBadge | null;
  attemptsRemaining: number;
  canRetake: boolean;
  bestPassingScore: number | null;
}
