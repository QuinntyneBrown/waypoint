import { DOCUMENT } from '@angular/common';
import { inject, Injectable } from '@angular/core';
import { DisplayPreferences } from '@waypoint/api';

/**
 * Applies display & accessibility preferences to the document root as
 * token/class hooks (see w-ui a11y.css). Used for the live preview on the
 * settings page and to re-apply saved preferences app-wide after sign-in.
 */
@Injectable({ providedIn: 'root' })
export class DisplayPreferencesService {
  private readonly doc = inject(DOCUMENT);

  apply(prefs: DisplayPreferences): void {
    const root = this.doc.documentElement;
    root.style.setProperty('--ph-text-scale', String((prefs.textScalePercent || 100) / 100));
    root.classList.toggle('ph-a11y-contrast', prefs.highContrast);
    root.classList.toggle('ph-a11y-reduce-motion', prefs.reduceMotion);
    root.classList.toggle('ph-a11y-underline', prefs.underlineLinks);
    root.classList.toggle('ph-a11y-reading-font', prefs.readingFont === 'serif');
    // No data-theme means the tokens follow the device colour scheme.
    if (prefs.theme === 'light' || prefs.theme === 'dark') {
      root.setAttribute('data-theme', prefs.theme);
    } else {
      root.removeAttribute('data-theme');
    }
  }
}
