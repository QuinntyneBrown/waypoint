import { expect } from '@playwright/test';
import { BasePage } from './base.page';

/** `/auth/sign-in` — the email/password sign-in form. */
export class SignInPage extends BasePage {
  static readonly path = '/auth/sign-in';

  get email() {
    return this.page.getByLabel('Email');
  }
  get password() {
    return this.page.getByLabel('Password', { exact: true });
  }
  get submitButton() {
    return this.page.getByRole('button', { name: /sign in/i });
  }
  get errorAlert() {
    return this.page.getByRole('alert');
  }
  /** Icon-only show/hide control on the password field. */
  get showPasswordButton() {
    return this.page.getByRole('button', { name: /show password/i });
  }

  async goto(): Promise<void> {
    await this.page.goto(SignInPage.path);
  }

  /** The sign-in action as a library primary button (`button[w-button]` host classes). */
  get primaryAction() {
    return this.page.locator('button.w-btn.w-btn--primary', { hasText: /sign in/i });
  }

  /** Fill and label colours of the primary action (design-system check). */
  primaryActionColors(): Promise<{ background: string; color: string }> {
    return this.submitButton.evaluate((el) => {
      const style = getComputedStyle(el);
      return { background: style.backgroundColor, color: style.color };
    });
  }

  /** Fill both fields and submit (assumes the form is already on screen). */
  async submitCredentials(email: string, password: string): Promise<void> {
    await this.email.fill(email);
    await this.password.fill(password);
    await this.submitButton.click();
  }

  /**
   * Full sign-in from a fresh navigation, used as a precondition by most
   * authenticated specs. Waits until the app routes away from the form.
   */
  async signIn(creds: { email: string; password: string }): Promise<void> {
    await this.goto();
    await this.submitCredentials(creds.email, creds.password);
    await expect(this.page).not.toHaveURL(/\/auth\/sign-in/);
  }
}

/** `/auth/account-locked` — shown after the failed-attempt threshold trips. */
export class AccountLockedPage extends BasePage {
  static readonly urlPattern = /\/auth\/account-locked/;

  get heading() {
    return this.page.getByRole('heading', { name: /too many sign-in attempts/i });
  }
  get lockoutDurationText() {
    return this.page.getByText(/15 minutes/i);
  }
  get resetPasswordLink() {
    return this.page.getByRole('link', { name: /reset password/i });
  }
  get contactMentorLink() {
    return this.page.getByRole('link', { name: /contact your mentor/i });
  }
}

/** `/auth/account-deactivated` — rejected sign-in with an anonymous recovery path. */
export class AccountDeactivatedPage extends BasePage {
  static readonly path = '/auth/account-deactivated';

  get heading() {
    return this.page.getByRole('heading', { name: /account is inactive/i });
  }
  get contactDirectorLink() {
    return this.page.getByRole('link', { name: /contact the director/i });
  }

  async goto(): Promise<void> {
    await this.page.goto(AccountDeactivatedPage.path);
  }
}

/** `/auth/forgot-password` — request a reset link. */
export class ForgotPasswordPage extends BasePage {
  static readonly path = '/auth/forgot-password';

  get email() {
    return this.page.getByLabel('Email');
  }
  get sendButton() {
    return this.page.getByRole('button', { name: /send reset link/i });
  }

  async goto(): Promise<void> {
    await this.page.goto(ForgotPasswordPage.path);
  }

  /** Navigate fresh, enter an email, and submit the request. */
  async requestReset(email: string): Promise<void> {
    await this.goto();
    await this.email.fill(email);
    await this.sendButton.click();
  }
}

/** `/auth/reset-link-sent` — enumeration-safe confirmation with a masked email. */
export class ResetLinkSentPage extends BasePage {
  static readonly urlPattern = /\/auth\/reset-link-sent/;

  get resendButton() {
    return this.page.getByRole('button', { name: /resend/i });
  }

  /** The masked form of an address, e.g. `s•••@example.com`. */
  maskedEmail(email: string) {
    return this.page.getByText(`${email[0]}•••@${email.split('@')[1]}`);
  }

  /** The unmasked address — asserted absent so nothing leaks. */
  exactEmail(email: string) {
    return this.page.getByText(email, { exact: true });
  }
}

/** `/auth/reset-password?token=…` — set a new password from a reset link. */
export class ResetPasswordPage extends BasePage {
  get newPassword() {
    return this.page.getByLabel('New password', { exact: true });
  }
  get confirmPassword() {
    return this.page.getByLabel('Confirm password', { exact: true });
  }
  get submitButton() {
    return this.page.getByRole('button', { name: /reset password/i });
  }
  get showPasswordButton() {
    return this.page.getByRole('button', { name: /show password/i });
  }
  /** Policy feedback shown for a too-short password. */
  get policyHint() {
    return this.page.getByText(/at least 8 characters/i);
  }
  /** Shown when the two password fields differ. */
  get mismatchError() {
    return this.page.getByText(/do not match/i);
  }

  async goto(token: string): Promise<void> {
    await this.page.goto(`/auth/reset-password?token=${token}`);
  }

  /** Fill both fields (confirm defaults to match) and submit. */
  async submit(newPassword: string, confirm: string = newPassword): Promise<void> {
    await this.newPassword.fill(newPassword);
    await this.confirmPassword.fill(confirm);
    await this.submitButton.click();
  }
}

/** `/auth/reset-link-invalid` — a bad or expired reset token lands here. */
export class ResetLinkInvalidPage extends BasePage {
  static readonly urlPattern = /\/auth\/reset-link-invalid/;

  get heading() {
    return this.page.getByRole('heading', { name: /(invalid|expired)/i });
  }
}

/** `/session-expired` — forced teardown after a refresh-token failure. */
export class SessionExpiredPage extends BasePage {
  static readonly urlPattern = /\/session-expired/;

  get heading() {
    return this.page.getByRole('heading', { name: /signed out/i });
  }
  get signInAgainLink() {
    return this.page.getByRole('link', { name: /sign in again/i });
  }
}

/** `/auth/invite?token=…` — anonymous invitation acceptance and recovery. */
export class InviteAcceptPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
  get displayName() {
    return this.page.getByLabel('Display name');
  }
  get password() {
    return this.page.getByLabel('Create password');
  }
  get confirmPassword() {
    return this.page.getByLabel('Confirm password');
  }
  get guidelines() {
    return this.page.getByRole('checkbox', { name: /community guidelines/i });
  }
  get joinButton() {
    return this.page.getByRole('button', { name: /join waypoint/i });
  }
  async agreeToGuidelines(): Promise<void> {
    await this.page.locator('label.w-check', { hasText: 'community guidelines' }).click();
  }
  async goto(token: string): Promise<void> {
    await this.page.goto(`/auth/invite?token=${encodeURIComponent(token)}`);
  }
}
