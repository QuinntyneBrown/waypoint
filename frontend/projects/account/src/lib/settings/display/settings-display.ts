import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  DEFAULT_DISPLAY_PREFERENCES,
  DisplayPreferences,
  DisplayPreferencesService as DisplayPreferencesApiService,
  TEXT_SCALE_MAX,
  TEXT_SCALE_MIN,
  Theme,
} from '@waypoint/api';
import { ToastService, WButton, WFormField, WHelp, WLabel } from '@waypoint/components';
import { SettingsShell } from '../settings-shell';
import { DisplayPreferencesService } from '@waypoint/core';

// Traces to: L2-PLATFORM-004, L2-PLATFORM-013
@Component({
  selector: 'w-settings-display',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [SettingsShell, WButton, WFormField, WLabel, WHelp],
  templateUrl: './settings-display.html',
})
export class SettingsDisplay {
  private readonly api = inject(DisplayPreferencesApiService);
  private readonly applier = inject(DisplayPreferencesService);
  private readonly toasts = inject(ToastService);

  protected readonly prefs = signal<DisplayPreferences>({ ...DEFAULT_DISPLAY_PREFERENCES });
  protected readonly saving = signal(false);
  protected readonly min = TEXT_SCALE_MIN;
  protected readonly max = TEXT_SCALE_MAX;
  protected readonly themes: { value: Theme; label: string }[] = [
    { value: 'system', label: 'System' },
    { value: 'light', label: 'Light' },
    { value: 'dark', label: 'Dark' },
  ];

  constructor() {
    // Load and apply the saved preferences (also proves persistence on reload).
    this.api.get().subscribe({
      next: (p) => {
        this.prefs.set(p);
        this.applier.apply(p);
      },
      error: () => this.applier.apply(this.prefs()),
    });
  }

  private update(patch: Partial<DisplayPreferences>): void {
    const next = { ...this.prefs(), ...patch };
    this.prefs.set(next);
    this.applier.apply(next); // live preview
  }

  protected changeScale(delta: number): void {
    const next = Math.min(this.max, Math.max(this.min, this.prefs().textScalePercent + delta));
    this.update({ textScalePercent: next });
  }
  protected setTheme(theme: Theme): void {
    this.update({ theme });
  }
  protected toggleContrast(): void {
    this.update({ highContrast: !this.prefs().highContrast });
  }
  protected toggleMotion(): void {
    this.update({ reduceMotion: !this.prefs().reduceMotion });
  }
  protected toggleUnderline(): void {
    this.update({ underlineLinks: !this.prefs().underlineLinks });
  }
  protected toggleReadingFont(): void {
    this.update({ readingFont: this.prefs().readingFont === 'serif' ? null : 'serif' });
  }

  protected save(): void {
    this.saving.set(true);
    this.api.save(this.prefs()).subscribe({
      next: (p) => {
        this.prefs.set(p);
        this.applier.apply(p);
        this.saving.set(false);
        this.toasts.success('Display settings saved.');
      },
      error: () => {
        this.saving.set(false);
        this.toasts.error('Couldn’t save your settings. Please try again.');
      },
    });
  }
}
