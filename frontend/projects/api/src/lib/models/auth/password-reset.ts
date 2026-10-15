export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

/**
 * Canonical password policy (mirrors backend `PasswordPolicy`, L2-AUTH-008):
 * at least 8 characters including at least one letter and one number. Enforced
 * identically at reset and change-password.
 */
export const PASSWORD_POLICY_MESSAGE =
  'Use at least 8 characters, including a letter and a number.';

export function isPasswordCompliant(password: string): boolean {
  return password.length >= 8 && /[A-Za-z]/.test(password) && /[0-9]/.test(password);
}

/** Mask an email for enumeration-safe confirmation copy: `j•••@example.com`. */
export function maskEmail(email: string): string {
  const at = email.indexOf('@');
  if (at <= 0) return email;
  return `${email.slice(0, 1)}•••@${email.slice(at + 1)}`;
}
