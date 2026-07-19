import { BasePage } from './base.page';

/** `/onboarding` — the first-run wizard (profile → cohort → notifications). */
export class OnboardingPage extends BasePage {
  static readonly urlPattern = /\/onboarding/;

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
  get nextButton() {
    return this.page.getByRole('button', { name: /next/i });
  }
  get finishButton() {
    return this.page.getByRole('button', { name: /finish/i });
  }
  get skipButton() {
    return this.page.getByRole('button', { name: /skip for now/i });
  }
  /** Step 2 notice shown for a youth not yet placed in a cohort. */
  get unplacedNotice() {
    return this.page.getByText(/not placed in a cohort yet/i);
  }

  checkbox(name: string) {
    return this.page.getByRole('checkbox', { name });
  }

  async next(): Promise<void> {
    await this.nextButton.click();
  }
  async finish(): Promise<void> {
    await this.finishButton.click();
  }
  async skip(): Promise<void> {
    await this.skipButton.click();
  }
}
