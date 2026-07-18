/** Program-administration people-directory models (L2-ADMIN-001). */

// Traces to: L2-ADMIN-001 (AC1 people listed with role/cohort/mentor/status/last-active)
export interface PersonSummary {
  id: string;
  displayName: string;
  email: string;
  role: string;
  status: string;
  cohortId: string | null;
  mentorId: string | null;
  lastActive: string | null;
}

export interface PagedPeople {
  items: PersonSummary[];
  total: number;
  page: number;
  pageSize: number;
}

export interface ListPeopleParams {
  page?: number;
  pageSize?: number;
  role?: string | null;
  cohortId?: string | null;
  search?: string | null;
}

export interface DirectoryCohort {
  id: string;
  name: string;
}
