/** Canonical system roles (backend `Roles`). Stored user roles only. */
export type WRoleName = 'Youth' | 'Mentor' | 'ProgramAdministrator' | 'Guardian' | 'Volunteer';

/** Session issued by the backend. Tokens live only in the JSON body — no cookies. */
export interface AuthResult {
  accessToken: string;
  /** ISO-8601 access-token expiry (UTC). */
  expiresAt: string;
  refreshToken: string;
  userId: string;
  email: string;
  displayName: string;
  role: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

export interface SignInRequest {
  email: string;
  password: string;
}

export interface RefreshRequest {
  refreshToken: string;
}
