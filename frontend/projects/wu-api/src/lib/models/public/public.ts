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

// Traces to: L2-PUB-003 (AC3 donation pledge; external PCI provider, no card data stored)
export interface SubmitDonationPledgeRequest {
  donorName: string;
  email: string;
  tier: string;
  monthlyAmount: number | null;
  frequency: string | null;
  /** Opaque reference returned by the external provider; never card/PAN data. */
  providerReference: string | null;
}

export interface SubmitDonationPledgeResult {
  donationPledgeId: string;
  acknowledgement: string;
}
