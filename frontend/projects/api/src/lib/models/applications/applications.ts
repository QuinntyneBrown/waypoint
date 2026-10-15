/** Admin application-review models (L2-INTAKE-002). */

// Traces to: L2-INTAKE-002 (AC1 admin review queue)
export interface ApplicationSummary {
  id: string;
  firstName: string;
  lastName: string;
  age: number;
  isMinor: boolean;
  neighbourhood: string;
  trackInterest: string | null;
  status: string;
  createdAt: string;
}

export interface ApplicationDetail extends ApplicationSummary {
  whoIsFilling: string;
  cohortInterest: string;
  whyInterested: string;
  guardianName: string | null;
  guardianEmail: string | null;
  guardianPhone: string | null;
  participationConsentAck: boolean;
  privacyAck: boolean;
  consentState: string;
  canApprove: boolean;
  cohortId: string | null;
  mentorId: string | null;
  decisionNote: string | null;
  reviewedAt: string | null;
}

export interface ApplicationOption {
  id: string;
  name: string;
}

export interface ApplicationMentorOption extends ApplicationOption {
  cohortIds: string[];
}

export interface ApplicationReviewOptions {
  cohorts: ApplicationOption[];
  mentors: ApplicationMentorOption[];
  tracks: ApplicationOption[];
}

export interface ApproveApplicationRequest {
  cohortId: string;
  mentorId: string;
  trackInterest: string | null;
  decisionNote: string | null;
}

export interface ApproveApplicationResult {
  youthUserId: string;
  status: string;
}
