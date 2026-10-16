/** `system` follows the device colour scheme (L2-PLATFORM-013). */
export type Theme = 'system' | 'light' | 'dark';

export interface DisplayPreferences {
  /** Body text scale as a percent (default 100). */
  textScalePercent: number;
  highContrast: boolean;
  reduceMotion: boolean;
  underlineLinks: boolean;
  /** Reading font key (e.g. 'serif') or null for the default. */
  readingFont: string | null;
  theme: Theme;
}

export const DEFAULT_DISPLAY_PREFERENCES: DisplayPreferences = {
  textScalePercent: 100,
  highContrast: false,
  reduceMotion: false,
  underlineLinks: false,
  readingFont: null,
  theme: 'system',
};

export const TEXT_SCALE_MIN = 90;
export const TEXT_SCALE_MAX = 150;
