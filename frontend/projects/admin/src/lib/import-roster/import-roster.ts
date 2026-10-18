import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ApiError,
  DataTransferService,
  RosterField,
  RosterImportRequest,
  RosterImportResult,
  RosterPreview,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  Button,
  ErrorText,
  FormField,
  Help,
  Icon,
  Label,
  Select,
  Shell,
  TextField,
  Alert,
  Card,
  CardHeader,
  CardTitle,
  CardBody,
  CardFooter,
  Badge,
  TableWrap,
  Dialog,
  DialogHeader,
  DialogTitle,
  DialogBody,
  DialogFooter,
} from '@waypoint/components';
import { finalize } from 'rxjs/operators';

interface ImportColumn {
  header: string;
  sample: string;
  field: RosterField;
}

// Traces to: L2-REPORT-004, L2-NFR-001, L2-NFR-009/010, L2-CONSENT-003
@Component({
  selector: 'w-import-roster',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Shell,
    Icon,
    FormsModule,
    Button,
    FormField,
    Label,
    Help,
    ErrorText,
    TextField,
    Select,
    Alert,
    Card,
    CardHeader,
    CardTitle,
    CardBody,
    CardFooter,
    Badge,
    TableWrap,
    Dialog,
    DialogHeader,
    DialogTitle,
    DialogBody,
    DialogFooter,
  ],
  templateUrl: './import-roster.html',
  styleUrl: './import-roster.css',
})
export class ImportRosterPage {
  protected readonly auth = inject(AuthStore);
  private readonly fileInput = viewChild('fileInput', { read: ElementRef<HTMLInputElement> });
  private readonly api = inject(DataTransferService);

  protected readonly fields: readonly { value: RosterField; label: string }[] = [
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
      return 'Map each Waypoint field at most once.';
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

  protected statusBadge(status: string): 'success' | 'warning' | 'error' {
    if (status === 'Ready') return 'success';
    return status === 'Invalid' ? 'error' : 'warning';
  }

  protected removeFile(): void {
    const input = this.fileInput()?.nativeElement;
    if (input) input.value = '';
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
