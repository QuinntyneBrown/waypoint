import { AuthResult, WRoleName } from './auth';

export type InvitationState = 'valid' | 'expired' | 'revoked' | 'accepted' | 'invalid';

export interface InvitationSummary {
  id: string;
  email: string;
  role: WRoleName;
  cohortId: string | null;
  cohortName: string | null;
  mentorId: string | null;
  mentorName: string | null;
  status: string;
  createdAt: string;
  expiresAt: string;
  createdByName: string;
}

export interface InvitationPage {
  items: InvitationSummary[];
  page: number;
  pageSize: number;
  total: number;
}

export interface InvitationListParams {
  page?: number;
  pageSize?: number;
  status?: string;
  role?: string;
  cohortId?: string;
  search?: string;
}

export interface InvitationContext {
  state: InvitationState;
  role: WRoleName | null;
  cohortName: string | null;
  mentorName: string | null;
  expiresAt: string | null;
  maskedEmail: string | null;
}

export interface InvitationOption {
  id: string;
  name: string;
}

export interface InvitationMentorOption {
  id: string;
  displayName: string;
}

export interface InvitationOptions {
  cohorts: InvitationOption[];
  mentors: InvitationMentorOption[];
}

export interface CreateInvitationRequest {
  email: string;
  role: WRoleName;
  cohortId?: string | null;
  mentorId?: string | null;
}

export interface BulkInvitationRequest {
  emails: string[];
  role: WRoleName;
  cohortId?: string | null;
  mentorId?: string | null;
}

export interface AcceptInvitationRequest {
  token: string;
  displayName: string;
  password: string;
  agreeToGuidelines: boolean;
}

export interface InvitationResult {
  invitationId: string;
}

export interface BulkInvitationResult {
  created: number;
}

export type InvitationAuthResult = AuthResult;
