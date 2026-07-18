// Traces to: L2-ADMIN-005, L2-REPORT-001
export interface MentorAssignmentOverview {
  mentors: MentorLoad[];
  youth: YouthMentorAssignment[];
  cohorts: CohortMentorAssignment[];
}

export interface MentorLoad {
  mentorId: string;
  mentorName: string;
  email: string;
  caseload: number;
  checkIns: number;
  reviews: number;
  targetMinimum: number;
  targetMaximum: number;
  loadBalance: 'Under target' | 'Balanced' | 'Overloaded';
  overloaded: boolean;
}

export interface YouthMentorAssignment {
  id: string;
  name: string;
  email: string;
  cohortId: string | null;
  cohortName: string | null;
  mentorId: string | null;
  mentorName: string | null;
}

export interface CohortMentorAssignment {
  id: string;
  name: string;
  leadMentorId: string | null;
  leadMentorName: string | null;
  supportingMentors: MentorAssignmentOption[];
}

export interface MentorAssignmentOption {
  id: string;
  name: string;
}
