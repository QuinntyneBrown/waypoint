export interface OnboardingStatus {
  displayName: string;
  pronouns: string | null;
  cohortId: string | null;
  mentorId: string | null;
  onboardingCompleted: boolean;
}

export interface CompleteOnboardingRequest {
  displayName: string;
  pronouns?: string | null;
  photoUrl?: string | null;
}
