export type DataSubjectAction = 'access' | 'correction' | 'removal' | 'consent-withdrawal';

export interface MyData {
  id: string;
  displayName: string;
  email: string;
  role: string;
  status: string;
  pronouns: string | null;
  photoUrl: string | null;
  cohortId: string | null;
  mentorId: string | null;
  guardianUserId: string | null;
  createdAt: string;
  lastSignInAt: string | null;
}

export interface DataCorrection {
  displayName?: string | null;
  email?: string | null;
  pronouns?: string | null;
  clearPronouns?: boolean;
}

export interface CreateDataSubjectRequest {
  actionType: DataSubjectAction;
  details?: string | null;
  correction?: DataCorrection | null;
}

export interface DataSubjectRequestResult {
  id: string;
  reference: string;
  subjectUserId: string;
  actionType: DataSubjectAction;
  status: string;
  requestedAt: string;
  completedAt?: string | null;
  resolutionNote?: string | null;
  correction?: DataCorrection | null;
}

export interface MyDataSubjectRequestPage {
  items: DataSubjectRequestResult[];
  page: number;
  pageSize: number;
  total: number;
  canExport: boolean;
}

export interface AdminDataSubjectRequest extends DataSubjectRequestResult {
  subjectName: string;
  subjectEmail: string;
  details: string | null;
  completedByUserId: string | null;
}

export interface AdminDataSubjectRequestPage {
  items: AdminDataSubjectRequest[];
  page: number;
  pageSize: number;
  total: number;
}

export interface FulfillDataSubjectRequest {
  displayName?: string | null;
  email?: string | null;
  pronouns?: string | null;
  clearPronouns?: boolean | null;
  resolutionNote?: string | null;
  verifiedOwnershipAttested: boolean;
  verifiedContactRequestIds?: string[];
  verifiedVolunteerSignupIds?: string[];
  verifiedDonationPledgeIds?: string[];
  verifiedApplicantApplicationIds?: string[];
  verifiedGuardianApplicationIds?: string[];
}
