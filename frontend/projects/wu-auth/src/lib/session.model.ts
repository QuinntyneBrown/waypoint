/** User snapshot kept in the session (never includes secrets beyond the refresh token). */
export interface SessionUser {
  userId: string;
  email: string;
  displayName: string;
  role: string;
}

/** Persisted session envelope (`wu.session.v1`): refresh token + user snapshot. */
export interface StoredSession {
  version: 1;
  refreshToken: string;
  user: SessionUser;
  /** Whether "remember me" was checked — selects local vs session storage. */
  remember: boolean;
}
