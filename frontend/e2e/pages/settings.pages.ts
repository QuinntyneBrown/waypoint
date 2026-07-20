import { BasePage } from './base.page';

/** `/settings/display` — text size, contrast, motion, reading font. */
export class DisplaySettingsPage extends BasePage {
  static readonly path = '/settings/display';

  get increaseTextSizeButton() {
    return this.page.getByRole('button', { name: 'Increase text size' });
  }
  get textScaleReadout() {
    return this.page.getByTestId('text-scale');
  }
  get saveButton() {
    return this.page.getByRole('button', { name: /save settings/i });
  }
  get savedToast() {
    return this.page.getByText(/display settings saved/i);
  }

  async goto(): Promise<void> {
    await this.page.goto(DisplaySettingsPage.path);
  }

  /**
   * Toggle a `wu-switch` by its accessible label. The native input is visually
   * hidden behind the custom control, so the click targets the label.
   */
  async toggleSwitch(label: string): Promise<void> {
    await this.page
      .locator('label.wu-switch', { has: this.page.locator(`[aria-label="${label}"]`) })
      .click();
  }

  async save(): Promise<void> {
    await this.saveButton.click();
  }

  /** The live `--wu-text-scale` custom property on the document root. */
  textScaleToken(): Promise<string> {
    return this.page.evaluate(() =>
      document.documentElement.style.getPropertyValue('--wu-text-scale'),
    );
  }

  /** Whether the app-wide high-contrast class is applied to the document root. */
  hasContrastClass(): Promise<boolean> {
    return this.page.evaluate(() =>
      document.documentElement.classList.contains('wu-a11y-contrast'),
    );
  }
}

/** `/settings/notifications` — per-category notification preferences. */
export class NotificationsSettingsPage extends BasePage {
  static readonly path = '/settings/notifications';

  get savedToast() {
    return this.page.getByText(/notification preferences saved/i);
  }

  checkbox(name: string) {
    return this.page.getByRole('checkbox', { name });
  }

  async goto(): Promise<void> {
    await this.page.goto(NotificationsSettingsPage.path);
  }

  /**
   * Toggle a `wu-check` by its accessible label. The native input is visually
   * hidden behind the custom box, so the click targets the label.
   */
  async toggleCheck(label: string): Promise<void> {
    await this.page
      .locator('label.wu-check', { has: this.page.locator(`[aria-label="${label}"]`) })
      .click();
  }
}

/** `/settings/profile` — profile fields and the change-password form. */
export class ProfileSettingsPage extends BasePage {
  static readonly path = '/settings/profile';

  get displayName() {
    return this.page.getByLabel('Display name');
  }
  get pronouns() {
    return this.page.getByLabel('Pronouns');
  }
  get photoInput() {
    return this.page.getByLabel(/profile photo/i);
  }
  get photoPreview() {
    return this.page.getByRole('img', { name: /profile photo preview/i });
  }
  get removePhotoButton() {
    return this.page.getByRole('button', { name: /remove photo/i });
  }
  get saveProfileButton() {
    return this.page.getByRole('button', { name: /save profile/i });
  }
  get profileUpdatedToast() {
    return this.page.getByText(/profile updated/i);
  }

  get currentPassword() {
    return this.page.getByLabel('Current password');
  }
  get newPassword() {
    return this.page.getByLabel('New password', { exact: true });
  }
  get confirmNewPassword() {
    return this.page.getByLabel('Confirm new password', { exact: true });
  }
  get changePasswordButton() {
    return this.page.getByRole('button', { name: /change password/i });
  }
  /** Policy feedback shown for a too-short new password. */
  get passwordPolicyHint() {
    return this.page.getByText(/at least 8 characters/i);
  }
  /** Shown when the new-password fields differ. */
  get passwordMismatchError() {
    return this.page.getByText(/do not match/i);
  }

  async goto(): Promise<void> {
    await this.page.goto(ProfileSettingsPage.path);
  }
}

/** `/support` — contact/support request form. */
export class SupportPage extends BasePage {
  static readonly path = '/support';

  get message() {
    return this.page.getByLabel(/how can we help/i);
  }
  get replyToEmail() {
    return this.page.getByLabel(/reply-to email/i);
  }
  get screenshotInput() {
    return this.page.getByLabel(/optional screenshot/i);
  }
  get removeScreenshotButton() {
    return this.page.getByRole('button', { name: /remove screenshot/i });
  }
  get sendButton() {
    return this.page.getByRole('button', { name: /send message/i });
  }
  get acknowledgementHeading() {
    return this.page.getByRole('heading', { name: /got your message/i });
  }

  async goto(): Promise<void> {
    await this.page.goto(SupportPage.path);
  }
}

/** `/help` — searchable, role-aware self-service content. */
export class HelpPage extends BasePage {
  static readonly path = '/help';
  get search() {
    return this.page.getByLabel('Search help');
  }
  get heading() {
    return this.page.getByRole('heading', { name: 'How can we help?', exact: true });
  }
  async goto(): Promise<void> {
    await this.page.goto(HelpPage.path);
  }
}

/** `/settings/privacy` — own-data view and trackable request form. */
export class PrivacySettingsPage extends BasePage {
  static readonly path = '/settings/privacy';
  get action() {
    return this.page.getByLabel('Request type');
  }
  get details() {
    return this.page.getByLabel('Details');
  }
  get submitButton() {
    return this.page.getByRole('button', { name: 'Submit request', exact: true });
  }
  get downloadButton() {
    return this.page.getByRole('button', { name: /download complete data export/i });
  }
  get loadMoreButton() {
    return this.page.getByRole('button', { name: 'Load more requests', exact: true });
  }
  async goto(): Promise<void> {
    await this.page.goto(PrivacySettingsPage.path);
  }
}
