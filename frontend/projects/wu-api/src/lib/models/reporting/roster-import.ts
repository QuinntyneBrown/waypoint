// Traces to: L2-REPORT-004
export type RosterField =
  'Name' | 'Email' | 'Cohort' | 'Mentor' | 'Track' | 'Status' | 'Consent' | 'Skip';

export interface RosterPreviewRow {
  line: number;
  name: string;
  email: string;
  cohort: string;
  mentor: string;
  track: string;
  status: 'Ready' | 'Duplicate' | 'MissingConsent' | 'Invalid';
  message: string | null;
}

export interface RosterPreview {
  rows: RosterPreviewRow[];
  ready: number;
  duplicates: number;
  needsConsent: number;
  invalid: number;
}

export interface RosterImportResult {
  imported: number;
  skippedDuplicates: number;
  needsConsent: number;
  invalid: number;
}

export interface RosterImportRequest {
  csvContent: string;
  columnMapping: Record<string, RosterField>;
}
