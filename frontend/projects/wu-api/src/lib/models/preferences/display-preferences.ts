export interface DisplayPreferences {
  /** Body text scale as a percent (default 100). */
  textScalePercent: number;
  highContrast: boolean;
  reduceMotion: boolean;
  underlineLinks: boolean;
  /** Reading font key (e.g. 'serif') or null for the default. */
  readingFont: string | null;
}

export const DEFAULT_DISPLAY_PREFERENCES: DisplayPreferences = {
  textScalePercent: 100,
  highContrast: false,
  reduceMotion: false,
  underlineLinks: false,
  readingFont: null,
};

export const TEXT_SCALE_MIN = 90;
export const TEXT_SCALE_MAX = 150;
