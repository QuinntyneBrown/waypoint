import { DOCUMENT } from '@angular/common';
import { inject, Injectable } from '@angular/core';
import { DisplayPreferences } from '@wu/api';

/**
 * Applies display & accessibility preferences to the document root as
 * token/class hooks (see wu-ui a11y.css). Used for the live preview on the
 * settings page and to re-apply saved preferences app-wide after sign-in.
 */
@Injectable({ providedIn: 'root' })
export class DisplayPreferencesService {
  private readonly doc = inject(DOCUMENT);

  apply(prefs: DisplayPreferences): void {
    const root = this.doc.documentElement;
    root.style.setProperty('--wu-text-scale', String((prefs.textScalePercent || 100) / 100));
    root.classList.toggle('wu-a11y-contrast', prefs.highContrast);
    root.classList.toggle('wu-a11y-reduce-motion', prefs.reduceMotion);
    root.classList.toggle('wu-a11y-underline', prefs.underlineLinks);
    root.classList.toggle('wu-a11y-reading-font', prefs.readingFont === 'serif');
  }
}
