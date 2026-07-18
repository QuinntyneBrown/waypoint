import { BasePage } from './base.page';

/**
 * The 404 state rendered by the catch-all route. Not a fixed path — navigate to
 * any unknown URL to reach it.
 */
export class NotFoundPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: /page not found/i });
  }
  get backToSafetyLink() {
    return this.page.getByRole('link', { name: /back to safety/i });
  }

  async goto(path: string): Promise<void> {
    await this.page.goto(path);
  }
}

/** API 403 state. */
export class ForbiddenPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: /don.t have access/i });
  }
}

/** API 500 state with retry/draft recovery. */
export class ServerErrorPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: /something went wrong/i });
  }
  get retryButton() {
    return this.page.getByRole('button', { name: /try again/i });
  }
}

/** `/consent-required` — youth recovery from the participation gate. */
export class ConsentRequiredPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: /guardian needs to sign off/i });
  }
  get notifyGuardianButton() {
    return this.page.getByRole('button', { name: /notify my guardian/i });
  }
  get guardianSignNowButton() {
    return this.page.getByRole('button', { name: /guardian can sign now/i });
  }
  get deliveryConfirmation() {
    return this.page.getByRole('status');
  }
}

/** Anonymous one-time guardian consent form. */
export class RequestedGuardianConsentPage extends BasePage {
  get heading() {
    return this.page.getByRole('heading', { name: /guardian consent/i });
  }
  get guardianName() {
    return this.page.getByLabel(/guardian full name/i);
  }
  get relationship() {
    return this.page.getByLabel(/relationship to youth/i);
  }
  get participationConsent() {
    return this.page.getByLabel(/i give permission.*participate/i);
  }
  get participationOption() {
    return this.page.getByText(/i give permission.*participate/i);
  }
  get mediaConsentOption() {
    return this.page.getByText(/allow word up to use photos or video/i);
  }
  get groupOnlyOption() {
    return this.page.getByText(/group photos only/i);
  }
  get signature() {
    return this.page.getByLabel(/type your full name as your signature/i);
  }
  get submitButton() {
    return this.page.getByRole('button', { name: /sign and submit consent/i });
  }
  get reference() {
    return this.page.getByTestId('public-consent-reference');
  }
}
