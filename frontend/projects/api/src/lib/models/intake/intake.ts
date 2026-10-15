export interface SubmitApplicationRequest {
  firstName: string;
  lastName: string;
  age: number;
  whoIsFilling: 'self' | 'guardian' | 'referrer';
  neighbourhood: string;
  cohortInterest: string;
  whyInterested: string;
  trackInterest?: string | null;
  guardianName?: string | null;
  guardianEmail?: string | null;
  guardianPhone?: string | null;
  participationConsentAck: boolean;
  privacyAck: boolean;
}

export interface SubmitApplicationResult {
  applicationId: string;
}

/** Eligible applicant age range (L2-INTAKE-001 AC3, inclusive). */
export const APPLICANT_MIN_AGE = 12;
export const APPLICANT_MAX_AGE = 18;
