import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ApiError,
  DataTransferApi,
  RosterField,
  RosterImportRequest,
  RosterImportResult,
  RosterPreview,
} from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuAccessibleDialog, WuIcon, WuShell } from '@wu/ui';
import { finalize } from 'rxjs/operators';

interface ImportColumn {
  header: string;
  sample: string;
  field: RosterField;
}

// Traces to: L2-REPORT-004, L2-NFR-001, L2-NFR-009/010, L2-CONSENT-003
@Component({
  selector: 'wu-import-roster',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, WuIcon, WuAccessibleDialog, FormsModule],
  templateUrl: './import-roster.html',
  styles: `
    .import-columns,
    .import-preview {
      width: 100%;
      border-collapse: collapse;
    }
    .import-columns th,
    .import-columns td,
    .import-preview th,
    .import-preview td {
      padding: var(--wu-sp-3);
      border-bottom: 1px solid var(--wu-border);
      text-align: left;
      vertical-align: top;
    }
    .import-file {
      display: flex;
      flex-wrap: wrap;
      gap: var(--wu-sp-3);
      align-items: center;
      justify-content: space-between;
    }
    .import-counts {
      display: grid;
      grid-template-columns: repeat(4, minmax(0, 1fr));
      gap: var(--wu-sp-3);
    }
    .import-count strong {
      display: block;
      font-size: 1.5rem;
    }
    .import-status {
      font-weight: 700;
    }
    .import-status--ready {
      color: var(--wu-success);
    }
    .import-status--duplicate,
    .import-status--consent {
      color: var(--wu-warning);
    }
    .import-status--invalid {
      color: var(--wu-danger);
    }
    .import-dialog-backdrop {
      position: fixed;
      z-index: 1000;
      inset: 0;
      display: grid;
      padding: var(--wu-sp-4);
      place-items: center;
      background: rgb(0 0 0 / 55%);
    }
    .import-dialog {
      width: min(32rem, 100%);
      margin: 0;
      border: 0;
    }
    @media (max-width: 48rem) {
      .import-counts {
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }
      .import-columns thead,
      .import-preview thead {
        display: none;
      }
      .import-columns tr,
      .import-preview tr {
        display: grid;
        gap: var(--wu-sp-2);
        padding: var(--wu-sp-3);
        border-bottom: 1px solid var(--wu-border);
      }
      .import-columns td,
      .import-preview td {
        display: grid;
        grid-template-columns: minmax(7rem, 0.8fr) minmax(0, 1.2fr);
        gap: var(--wu-sp-2);
        padding: 0;
        border: 0;
        overflow-wrap: anywhere;
      }
      .import-columns td::before,
      .import-preview td::before {
        content: attr(data-label);
        color: var(--wu-text-muted);
        font-weight: 700;
      }
      .import-columns .wu-select,
      .import-file .wu-btn,
      .import-actions .wu-btn {
        min-height: 45px;
        width: 100%;
      }
    }
  `,
})
export class ImportRosterPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(DataTransferApi);

  protected readonly fields: ReadonlyArray<{ value: RosterField; label: string }> = [
    { value: 'Name', label: 'Name' },
    { value: 'Email', label: 'Email' },
    { value: 'Cohort', label: 'Cohort' },
    { value: 'Mentor', label: 'Mentor' },
    { value: 'Track', label: 'Track' },
    { value: 'Status', label: 'Status' },
    { value: 'Consent', label: 'Consent' },
    { value: 'Skip', label: '— Skip column —' },
  ];
  protected readonly fileName = signal<string | null>(null);
  protected readonly fileSize = signal(0);
  protected readonly rowCount = signal(0);
  protected readonly columns = signal<ImportColumn[]>([]);
  protected readonly preview = signal<RosterPreview | undefined>(undefined);
  protected readonly result = signal<RosterImportResult | undefined>(undefined);
  protected readonly loading = signal(false);
  protected readonly confirming = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  private readonly csvContent = signal('');

  protected async chooseFile(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    this.resetState();
    if (!file) return;
    if (!file.name.toLowerCase().endsWith('.csv')) {
      this.errorMessage.set('Choose a .csv file. Other file types are not accepted.');
      input.value = '';
      return;
    }
    if (file.size > 2 * 1024 * 1024) {
      this.errorMessage.set('Choose a CSV file that is 2 MB or smaller.');
      input.value = '';
      return;
    }
    const allowedTypes = ['', 'text/csv', 'application/vnd.ms-excel'];
    if (!allowedTypes.includes(file.type.toLowerCase())) {
      this.errorMessage.set('Choose a .csv file with a CSV content type.');
      input.value = '';
      return;
    }

    const content = await file.text();
    const parsed = this.parseCsv(content);
    const headers = parsed[0]?.map((value) => value.trim()) ?? [];
    const rows = parsed.slice(1).filter((row) => row.some((value) => value.trim().length > 0));
    if (headers.length === 0 || rows.length === 0 || headers.some((header) => !header)) {
      this.errorMessage.set('The CSV must contain a header row and at least one data row.');
      input.value = '';
      return;
    }
    if (new Set(headers.map((header) => header.toLowerCase())).size !== headers.length) {
      this.errorMessage.set('Every CSV column header must be unique.');
      input.value = '';
      return;
    }

    this.fileName.set(file.name);
    this.fileSize.set(file.size);
    this.rowCount.set(rows.length);
    this.csvContent.set(content);
    this.columns.set(
      headers.map((header, index) => ({
        header,
        sample: rows.find((row) => row[index]?.trim())?.[index]?.trim() ?? '—',
        field: this.suggestField(header),
      })),
    );
  }

  protected updateMapping(header: string, field: RosterField): void {
    this.columns.update((columns) =>
      columns.map((column) => (column.header === header ? { ...column, field } : column)),
    );
    this.preview.set(undefined);
    this.result.set(undefined);
  }

  protected mappingError(): string | null {
    const mapped = this.columns().map((column) => column.field);
    if (mapped.filter((field) => field === 'Email').length !== 1) {
      return 'Map exactly one column to Email before previewing.';
    }
    const assigned = mapped.filter((field) => field !== 'Skip');
    if (new Set(assigned).size !== assigned.length) {
      return 'Map each Word Up field at most once.';
    }
    return null;
  }

  protected previewRoster(): void {
    if (this.mappingError() || this.loading()) return;
    this.loading.set(true);
    this.errorMessage.set(null);
    this.preview.set(undefined);
    this.result.set(undefined);
    this.api
      .previewRoster(this.request())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (preview) => this.preview.set(preview),
        error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
      });
  }

  protected requestConfirmation(): void {
    if ((this.preview()?.ready ?? 0) > 0) this.confirming.set(true);
  }

  protected cancelConfirmation(): void {
    this.confirming.set(false);
  }

  protected confirmImport(): void {
    if (this.loading()) return;
    this.loading.set(true);
    this.errorMessage.set(null);
    this.api
      .importRoster(this.request())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (result) => {
          this.result.set(result);
          this.confirming.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(ApiError.from(error).title);
          this.confirming.set(false);
        },
      });
  }

  protected statusLabel(status: string): string {
    return status === 'MissingConsent' ? 'Missing consent' : status;
  }

  protected statusClass(status: string): string {
    return status === 'MissingConsent' ? 'consent' : status.toLowerCase();
  }

  protected removeFile(input: HTMLInputElement): void {
    input.value = '';
    this.resetState();
  }

  private request(): RosterImportRequest {
    return {
      csvContent: this.csvContent(),
      columnMapping: Object.fromEntries(
        this.columns().map((column) => [column.header, column.field]),
      ),
    };
  }

  private resetState(): void {
    this.fileName.set(null);
    this.fileSize.set(0);
    this.rowCount.set(0);
    this.columns.set([]);
    this.csvContent.set('');
    this.preview.set(undefined);
    this.result.set(undefined);
    this.confirming.set(false);
    this.errorMessage.set(null);
  }

  private suggestField(header: string): RosterField {
    const normalized = header.toLowerCase().replace(/[^a-z]/g, '');
    if (normalized === 'name' || normalized === 'fullname') return 'Name';
    if (normalized === 'email' || normalized === 'emailaddress') return 'Email';
    if (normalized === 'cohort' || normalized === 'group') return 'Cohort';
    if (normalized === 'mentor' || normalized === 'leader') return 'Mentor';
    if (normalized === 'track' || normalized === 'stream') return 'Track';
    if (normalized === 'status') return 'Status';
    if (normalized === 'consent') return 'Consent';
    return 'Skip';
  }

  private parseCsv(content: string): string[][] {
    const rows: string[][] = [];
    let row: string[] = [];
    let field = '';
    let quoted = false;
    for (let index = 0; index < content.length; index++) {
      const character = content[index];
      if (quoted) {
        if (character === '"' && content[index + 1] === '"') {
          field += '"';
          index++;
        } else if (character === '"') {
          quoted = false;
        } else {
          field += character;
        }
      } else if (character === '"') {
        quoted = true;
      } else if (character === ',') {
        row.push(field);
        field = '';
      } else if (character === '\n') {
        row.push(field);
        rows.push(row);
        row = [];
        field = '';
      } else if (character !== '\r') {
        field += character;
      }
    }
    row.push(field);
    if (row.length > 1 || row[0]) rows.push(row);
    return rows;
  }
}
