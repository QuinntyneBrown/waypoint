/** Public marketing-site form models (anonymous). */

// Traces to: L2-PUB-002 (authoritative published curriculum, cohorts, journey, and impact)
export interface PublicContentView {
  tracks: PublicTrackView[];
  cohorts: PublicCohortView[];
  mentorship: PublicMentorshipView;
  journey: PublicJourneyStepView[];
  impact: PublicImpactStatsView;
}

export interface PublicTrackView {
  id: string;
  name: string;
  key: string;
  description: string | null;
  modules: PublicModuleView[];
}

export interface PublicModuleView {
  id: string;
  name: string;
  description: string | null;
  order: number;
  outcomeBadge: PublicBadgeView | null;
}

export interface PublicBadgeView {
  id: string;
  name: string;
  icon: string;
  description: string | null;
}

export interface PublicCohortView {
  id: string;
  name: string;
  neighbourhood: string | null;
  termStart: string;
  termEnd: string;
  leadMentor: string | null;
  status: 'Upcoming' | 'Active' | 'Completed';
}

export interface PublicMentorshipView {
  summary: string;
  features: string[];
}

export interface PublicJourneyStepView {
  step: number;
  title: string;
  description: string;
}

export interface PublicImpactStatsView {
  tracks: number;
  cohorts: number;
  modules: number;
  neighbourhoods: number;
}

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
