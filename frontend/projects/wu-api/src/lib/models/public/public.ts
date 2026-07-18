/** Public marketing-site form models (anonymous). */

// Traces to: L2-PUB-003 (AC1 contact captured + acknowledged)
export interface SubmitContactRequest {
  name: string;
  email: string;
  topic: string;
  message: string;
}

export interface SubmitContactResult {
  contactRequestId: string;
  acknowledgement: string;
}

// Traces to: L2-PUB-003 (AC2 volunteer sign-up; screening-consent recorded)
export interface SubmitVolunteerSignupRequest {
  name: string;
  email: string;
  phone: string;
  areasOfInterest: string;
  screeningConsentAck: boolean;
}

export interface SubmitVolunteerSignupResult {
  volunteerSignupId: string;
  acknowledgement: string;
}
