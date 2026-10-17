import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  ApiError,
  InvitationMentorOption,
  InvitationOption,
  InvitationSummary,
  InvitationsService,
  WRoleName,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WButton, WErrorText, WFormField, WIcon, WLabel, WShell } from '@waypoint/components';
import { Observable } from 'rxjs';
import { shellRole } from '@waypoint/core';

const ROLES: readonly WRoleName[] = [
  'Youth',
  'Mentor',
  'ProgramAdministrator',
  'Guardian',
  'Volunteer',
];

// Traces to: L2-AUTH-003, L2-AUTH-004
@Component({
  selector: 'w-invitations',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, ReactiveFormsModule, WIcon, WShell, WButton, WFormField, WLabel, WErrorText],
  templateUrl: './invitations.html',
  styles: `
    .invite-layout {
      display: grid;
      gap: var(--w-s5);
      grid-template-columns: minmax(18rem, 0.7fr) minmax(0, 1.3fr);
    }
    .invite-filters {
      display: grid;
      gap: var(--w-s3);
      grid-template-columns: 2fr repeat(2, minmax(9rem, 1fr));
    }
    .invite-actions {
      align-items: center;
      display: flex;
      flex-wrap: wrap;
      gap: var(--w-s2);
    }
    .invite-email {
      overflow-wrap: anywhere;
    }
    @media (max-width: 62rem) {
      .invite-layout {
        grid-template-columns: 1fr;
      }
    }
    @media (max-width: 48rem) {
      .invite-filters {
        grid-template-columns: 1fr;
      }
    }
  `,
})
export class InvitationsPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(InvitationsService);
  private readonly fb = inject(FormBuilder);

  protected readonly shellRole = computed(() => shellRole(this.auth.role()));
  protected readonly isAdmin = computed(() => this.auth.role() === 'ProgramAdministrator');
  protected readonly roles = ROLES;
  protected readonly items = signal<InvitationSummary[]>([]);
  protected readonly cohorts = signal<InvitationOption[]>([]);
  protected readonly mentors = signal<InvitationMentorOption[]>([]);
  protected readonly loading = signal(true);
  protected readonly submitting = signal(false);
  protected readonly page = signal(1);
  protected readonly total = signal(0);
  protected readonly search = signal('');
  protected readonly status = signal('Pending');
  protected readonly roleFilter = signal('');
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly statusMessage = signal<string | null>(null);
  protected readonly revokeCandidate = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    emails: ['', Validators.required],
    role: ['Youth' as WRoleName, Validators.required],
    cohortId: [''],
    mentorId: [''],
  });

  constructor() {
    this.load();
    this.api.options().subscribe({
      next: (result) => {
        this.cohorts.set(result.cohorts);
        this.mentors.set(result.mentors);
      },
    });
  }

  protected applyFilters(): void {
    this.page.set(1);
    this.load();
  }

  protected searchChanged(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  protected statusChanged(event: Event): void {
    this.status.set((event.target as HTMLSelectElement).value);
    this.applyFilters();
  }

  protected roleFilterChanged(event: Event): void {
    this.roleFilter.set((event.target as HTMLSelectElement).value);
    this.applyFilters();
  }

  protected send(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const emails = [
      ...new Set(
        value.emails
          .split(/[\n,;]+/)
          .map((email) => email.trim().toLocaleLowerCase())
          .filter(Boolean),
      ),
    ];
    if (emails.length === 0 || emails.some((email) => !/^\S+@\S+\.\S+$/.test(email))) {
      this.errorMessage.set('Enter one valid email address per line.');
      return;
    }
    const role: WRoleName = this.isAdmin() ? value.role : 'Youth';
    const context = {
      role,
      cohortId: value.cohortId || null,
      mentorId: value.mentorId || (this.auth.role() === 'Mentor' ? this.auth.user()?.userId : null),
    };
    this.submitting.set(true);
    this.errorMessage.set(null);
    const operation: Observable<unknown> =
      emails.length === 1
        ? this.api.create({ email: emails[0], ...context })
        : this.api.bulk({ emails, ...context });
    operation.subscribe({
      next: () => {
        this.statusMessage.set(
          emails.length === 1 ? 'Invitation sent.' : `${emails.length} invitations sent.`,
        );
        this.form.controls.emails.reset('');
        this.submitting.set(false);
        this.status.set('Pending');
        this.load();
      },
      error: (error: unknown) => {
        this.errorMessage.set(ApiError.from(error).title);
        this.submitting.set(false);
      },
    });
  }

  protected resend(item: InvitationSummary): void {
    this.api.resend(item.id).subscribe({
      next: () => {
        this.statusMessage.set(`A fresh invitation was sent to ${item.email}.`);
        this.load();
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  protected confirmRevoke(item: InvitationSummary): void {
    this.revokeCandidate.set(item.id);
  }

  protected revoke(item: InvitationSummary): void {
    this.api.revoke(item.id).subscribe({
      next: () => {
        this.statusMessage.set(`The invitation for ${item.email} was revoked.`);
        this.revokeCandidate.set(null);
        this.load();
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  protected previous(): void {
    if (this.page() <= 1) return;
    this.page.update((value) => value - 1);
    this.load();
  }

  protected next(): void {
    if (this.page() * 25 >= this.total()) return;
    this.page.update((value) => value + 1);
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.api
      .list({
        page: this.page(),
        pageSize: 25,
        status: this.status(),
        role: this.roleFilter(),
        search: this.search(),
      })
      .subscribe({
        next: (result) => {
          this.items.set(result.items);
          this.total.set(result.total);
          this.loading.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(ApiError.from(error).title);
          this.loading.set(false);
        },
      });
  }
}
