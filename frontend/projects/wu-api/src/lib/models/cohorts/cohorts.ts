// Traces to: L2-ADMIN-003 (cohort creation, detail, editing, and membership)
export interface CohortSummary {
  id: string;
  name: string;
  trackId: string | null;
  trackName: string | null;
  mentorId: string | null;
  mentorName: string | null;
  startDate: string;
  endDate: string;
  location: string | null;
  memberCount: number;
}

export interface CohortDetail {
  id: string;
  name: string;
  trackId: string | null;
  trackName: string | null;
  leadMentorId: string | null;
  leadMentorName: string | null;
  startDate: string;
  endDate: string;
  location: string | null;
  memberCount: number;
  members: CohortYouth[];
  availableYouth: CohortYouth[];
}

export interface CohortYouth {
  id: string;
  name: string;
  email: string;
  status: string;
}

export interface CohortOptions {
  tracks: CohortOption[];
  mentors: CohortOption[];
}

export interface CohortOption {
  id: string;
  name: string;
}

export interface CreateCohortRequest {
  name: string;
  trackId: string;
  leadMentorId: string;
  startDate: string | null;
  endDate: string | null;
  location: string | null;
}

export interface EditCohortRequest {
  name: string;
  leadMentorId: string | null;
  location: string | null;
}

export interface CohortResult {
  id: string;
}
