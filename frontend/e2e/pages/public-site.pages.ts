import { BasePage } from './base.page';

/** `/` — the public marketing landing page (public-site). */
export class LandingPage extends BasePage {
  static readonly path = '/';

  get heroHeading() {
    return this.page.getByRole('heading', { name: /young leaders who love jesus/i });
  }
  /** A journey-step heading by exact title (Apply, Join a cohort, …). */
  journeyStep(step: string) {
    return this.page.getByRole('heading', { name: step, exact: true });
  }
  get signInLink() {
    return this.page.getByRole('link', { name: 'Sign in' });
  }
  get getInvolvedLink() {
    return this.page.getByRole('link', { name: 'Get involved' }).first();
  }
  get contactLink() {
    return this.page.getByRole('link', { name: 'Contact' }).first();
  }
  get applyCta() {
    return this.page.getByRole('link', { name: 'Apply', exact: true }).first();
  }
  impactStat(label: string) {
    return this.page
      .locator('.w-statcard')
      .filter({ hasText: label })
      .locator('.w-statcard__value');
  }

  /** Returns the navigation response so specs can assert the status code. */
  goto() {
    return this.page.goto(LandingPage.path);
  }
}

/** `/programs` — published tracks, modules, badges, and mentorship. */
export class ProgramsPage extends BasePage {
  static readonly path = '/programs';

  async goto(): Promise<void> {
    await this.page.goto(ProgramsPage.path);
  }

  track(name: string) {
    return this.page.getByRole('heading', { name, exact: true });
  }

  module(name: string) {
    return this.page.getByText(name, { exact: true });
  }

  outcomeBadge(name: string) {
    return this.page.getByText(name, { exact: true });
  }
}

/** `/about` — the public, privacy-safe current-cohort view. */
export class AboutPage extends BasePage {
  static readonly path = '/about';

  async goto(): Promise<void> {
    await this.page.goto(AboutPage.path);
  }

  cohort(name: string) {
    return this.page.getByRole('article').filter({
      has: this.page.getByRole('heading', { name, exact: true }),
    });
  }
}

/** `/apply` — the anonymous application intake form (public-site). */
export class ApplyPage extends BasePage {
  static readonly path = '/apply';

  get whoIsFilling() {
    return this.page.getByLabel(/who.s filling this out/i);
  }
  get cohortInterest() {
    return this.page.getByLabel(/cohort.*interested in/i);
  }
  get firstName() {
    return this.page.getByLabel('First name');
  }
  get lastName() {
    return this.page.getByLabel('Last name');
  }
  get age() {
    return this.page.getByLabel('Age');
  }
  get neighbourhood() {
    return this.page.getByLabel('Neighbourhood');
  }
  get interest() {
    return this.page.getByLabel(/why are you interested/i);
  }
  get guardianName() {
    return this.page.getByLabel('Guardian name');
  }
  get guardianEmail() {
    return this.page.getByLabel('Guardian email');
  }
  get guardianPhone() {
    return this.page.getByLabel('Guardian phone');
  }
  get submitButton() {
    return this.page.getByRole('button', { name: /submit application/i });
  }
  get confirmationHeading() {
    return this.page.getByRole('heading', { name: /application received/i });
  }
  /** Next-steps copy confirming no account is created at intake. */
  get accountNotice() {
    return this.page.getByText(/account/i);
  }
  get participationCheckbox() {
    return this.page.getByRole('checkbox', { name: /participation/i });
  }
  get privacyCheckbox() {
    return this.page.getByRole('checkbox', { name: /privacy notice/i });
  }
  get eligibleAgesError() {
    return this.page.getByText(/12.18 inclusive/i);
  }
  get participationError() {
    return this.page.getByText(/acknowledge participation consent/i);
  }
  get privacyError() {
    return this.page.getByText(/acknowledge the privacy notice/i);
  }
  get cohortInterestError() {
    return this.page.getByText(/choose a cohort/i);
  }

  /**
   * Navigate to the form. Pass an absolute `origin` (e.g. the public-site URL)
   * when driving from a project whose baseURL is a different app (portal-xs).
   */
  async goto(origin = ''): Promise<void> {
    // The public app is SSR + hydrated. Wait for browser bundles to settle so form events are
    // attached before tests interact with controls rendered in the server response.
    await this.page.goto(`${origin}${ApplyPage.path}`, { waitUntil: 'networkidle' });
  }

  /** Toggle the participation-consent acknowledgement via its custom checkbox. */
  async acknowledgeParticipation(): Promise<void> {
    await this.page.locator('label.w-check', { hasText: 'participation' }).click();
  }

  /** Toggle the privacy-notice acknowledgement via its custom checkbox box. */
  async acknowledgePrivacy(): Promise<void> {
    await this.page
      .locator('label.w-check', { hasText: 'privacy notice' })
      .locator('.w-check__box')
      .click();
  }
}

/** `/contact` — anonymous contact form (public-site). */
export class ContactPage extends BasePage {
  static readonly path = '/contact';

  get heading() {
    return this.page.getByRole('heading', { name: /get in touch/i });
  }
  get name() {
    return this.page.getByLabel('Your name');
  }
  get email() {
    return this.page.getByLabel('Email');
  }
  get topic() {
    return this.page.getByLabel('Topic');
  }
  get message() {
    return this.page.getByLabel('Message');
  }
  get sendButton() {
    return this.page.getByRole('button', { name: /send message/i });
  }
  get acknowledgementHeading() {
    return this.page.getByRole('heading', { name: /message sent/i });
  }
  get nameError() {
    return this.page.getByText(/enter your name/i);
  }
  get emailError() {
    return this.page.getByText(/valid email/i);
  }
  get messageError() {
    return this.page.getByText(/enter a message/i);
  }

  async goto(): Promise<void> {
    // Wait for hydration so a fast submit cannot fall through to native form navigation.
    await this.page.goto(ContactPage.path, { waitUntil: 'networkidle' });
  }
}

/** `/get-involved` — anonymous volunteer sign-up (public-site). */
export class GetInvolvedPage extends BasePage {
  static readonly path = '/get-involved';

  get name() {
    return this.page.getByLabel('Your name');
  }
  get email() {
    return this.page.getByLabel('Email');
  }
  get phone() {
    return this.page.getByLabel('Phone');
  }
  get areasOfInterest() {
    return this.page.getByLabel(/how would you like to help/i);
  }
  get screeningCheckbox() {
    return this.page.getByRole('checkbox', { name: /screening/i });
  }
  get submitButton() {
    return this.page.getByRole('button', { name: /sign up to volunteer/i });
  }
  get acknowledgementHeading() {
    return this.page.getByRole('heading', { name: /thanks for volunteering/i });
  }
  get screeningError() {
    return this.page.getByText(/acknowledge the screening/i);
  }

  async goto(): Promise<void> {
    // Wait for hydration so form input is reflected in Angular's reactive state.
    await this.page.goto(GetInvolvedPage.path, { waitUntil: 'networkidle' });
  }

  /** Toggle the screening-consent acknowledgement via its custom checkbox. */
  async acknowledgeScreening(): Promise<void> {
    await this.page.locator('label.w-check', { hasText: 'screening' }).click();
  }
}

/** `/donate` — donation tiers with hand-off to an external PCI provider. */
export class DonatePage extends BasePage {
  static readonly path = '/donate';

  get donorName() {
    return this.page.getByLabel('Your name');
  }
  get email() {
    return this.page.getByLabel('Email');
  }
  /** Any on-page card/CVC input — asserted absent (no card data collected). */
  get cardInputs() {
    return this.page.locator(
      'input[autocomplete="cc-number"], input[name*="card" i], input[name*="cvc" i]',
    );
  }
  tier(label: string) {
    return this.page.getByRole('button', { name: label });
  }
  get nameError() {
    return this.page.getByText(/enter your name/i);
  }
  get emailError() {
    return this.page.getByText(/valid email/i);
  }
  get continueButton() {
    return this.page.getByRole('button', { name: /continue to secure checkout/i });
  }
  get thankYou() {
    return this.page.getByText(/thank you/i);
  }
  /** Confirmation summary for a chosen monthly amount, e.g. `monthlySummary('50')`. */
  monthlySummary(amount: string) {
    return this.page.getByText(new RegExp(`\\$${amount} monthly`, 'i'));
  }
  get providerLink() {
    return this.page.getByRole('link', { name: /continue to secure provider/i });
  }

  async goto(): Promise<void> {
    await this.page.goto(DonatePage.path);
  }
}
