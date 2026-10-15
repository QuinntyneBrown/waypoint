// Traces to: L2-ADMIN-006
export interface OrganizationSettings {
  name: string;
  displayName: string | null;
  logoUrl: string | null;
  city: string | null;
  contactEmail: string | null;
  consentStatement: string | null;
  requireReConsentEachTerm: boolean;
  requireMediaConsent: boolean;
}

export interface ProgramTerm {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  status: 'Upcoming' | 'Active' | 'Completed';
}

export interface ProgramTrackSetting {
  id: string;
  name: string;
  key: string;
  status: string;
  enabled: boolean;
}

export interface SaveProgramTerm {
  name: string;
  startDate: string;
  endDate: string;
  status: ProgramTerm['status'];
}

export interface TermResult {
  id: string;
}
