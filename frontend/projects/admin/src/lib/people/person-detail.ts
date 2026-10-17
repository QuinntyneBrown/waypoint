import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiError, PeopleService, PersonDetail } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  WAccessibleDialog,
  WButton,
  WFormField,
  WIcon,
  WIconButton,
  WLabel,
  WLink,
  WSelect,
  WShell,
  WTextField,
  WAlert,
  WAlertTitle,
} from '@waypoint/components';
import { finalize, Observable } from 'rxjs';

type PersonTab = 'profile' | 'guardian' | 'progress' | 'attendance' | 'notes' | 'account';
type LifecycleAction =
  | { kind: 'role'; role: string }
  | { kind: 'deactivate' }
  | { kind: 'reactivate' }
  | { kind: 'remove' };

// Traces to: L2-ADMIN-002, L2-CONSENT-003
@Component({
  selector: 'w-person-detail',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    WShell,
    WIcon,
    WAccessibleDialog,
    RouterLink,
    DatePipe,
    WButton,
    WIconButton,
    WLink,
    WFormField,
    WLabel,
    WTextField,
    WSelect,
    WAlert,
    WAlertTitle,
  ],
  templateUrl: './person-detail.html',
  styles: `
    .record-grid {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: var(--w-s5);
    }
    .account-actions {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: var(--w-s4);
    }
    .account-actions .w-btn {
      justify-content: center;
      min-height: 45px;
    }
    .danger-zone {
      border-color: var(--w-error);
    }
    .confirmation-copy {
      overflow-wrap: anywhere;
    }
    @media (max-width: 42rem) {
      .record-grid,
      .account-actions {
        grid-template-columns: 1fr;
      }
      .w-modal__footer .w-btn {
        flex: 1 1 100%;
        min-height: 45px;
      }
    }
  `,
})
export class PersonDetailPage {
  protected readonly auth = inject(AuthStore);
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(PeopleService);
  private readonly personId = this.route.snapshot.paramMap.get('id') ?? '';

  protected readonly detail = signal<PersonDetail | undefined>(undefined);
  protected readonly activeTab = signal<PersonTab>('profile');
  protected readonly roleSelection = signal('');
  protected readonly pendingAction = signal<LifecycleAction | null>(null);
  protected readonly removeConfirmation = signal('');
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly outcome = signal<string | null>(null);
  protected readonly isOwnRecord = computed(() => this.auth.user()?.userId === this.detail()?.id);
  protected readonly tabs: readonly { key: PersonTab; label: string }[] = [
    { key: 'profile', label: 'Profile' },
    { key: 'guardian', label: 'Guardian & consent' },
    { key: 'progress', label: 'Progress' },
    { key: 'attendance', label: 'Attendance' },
    { key: 'notes', label: 'Notes' },
    { key: 'account', label: 'Account' },
  ];
  protected readonly roles = [
    { value: 'Youth', label: 'Youth' },
    { value: 'Mentor', label: 'Mentor' },
    { value: 'Guardian', label: 'Guardian' },
    { value: 'Volunteer', label: 'Volunteer' },
    { value: 'ProgramAdministrator', label: 'Program administrator' },
  ];

  constructor() {
    this.load();
  }

  protected selectTab(tab: PersonTab): void {
    this.activeTab.set(tab);
  }

  protected selectRole(role: string): void {
    this.roleSelection.set(role);
  }

  protected openRoleChange(): void {
    const person = this.detail();
    const role = this.roleSelection();
    if (!person || !role || role === person.role || person.status === 'Removed') return;
    this.pendingAction.set({ kind: 'role', role });
  }

  protected openAction(kind: 'deactivate' | 'reactivate' | 'remove'): void {
    this.removeConfirmation.set('');
    this.pendingAction.set({ kind });
  }

  protected cancelAction(): void {
    if (!this.saving()) {
      this.pendingAction.set(null);
      this.removeConfirmation.set('');
    }
  }

  protected confirmAction(): void {
    const person = this.detail();
    const action = this.pendingAction();
    if (!person || !action || this.saving()) return;
    if (action.kind === 'remove' && this.removeConfirmation() !== person.displayName) return;

    let request: Observable<void>;
    switch (action.kind) {
      case 'role':
        request = this.api.changeRole(person.id, action.role);
        break;
      case 'deactivate':
        request = this.api.deactivate(person.id);
        break;
      case 'reactivate':
        request = this.api.reactivate(person.id);
        break;
      case 'remove':
        request = this.api.remove(person.id);
        break;
    }

    this.saving.set(true);
    this.errorMessage.set(null);
    request.pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.outcome.set(this.successMessage(action, person));
        this.pendingAction.set(null);
        this.removeConfirmation.set('');
        this.load(false);
      },
      error: (error: unknown) => {
        this.pendingAction.set(null);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }

  protected sendPasswordReset(): void {
    const person = this.detail();
    if (!person || person.status === 'Removed' || this.saving()) return;
    this.saving.set(true);
    this.errorMessage.set(null);
    this.api
      .sendPasswordReset(person.id)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => this.outcome.set(`Password reset sent to ${person.email}.`),
        error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
      });
  }

  protected percent(value: number): number {
    return Math.round(value * 100);
  }

  protected isStaffRole(role: string): boolean {
    return role === 'Mentor' || role === 'Volunteer' || role === 'ProgramAdministrator';
  }

  protected actionTitle(action: LifecycleAction): string {
    switch (action.kind) {
      case 'role':
        return 'Change account role?';
      case 'deactivate':
        return 'Deactivate account?';
      case 'reactivate':
        return 'Reactivate account?';
      case 'remove':
        return 'Permanently remove person?';
    }
  }

  protected confirmLabel(action: LifecycleAction): string {
    switch (action.kind) {
      case 'role':
        return 'Confirm role change';
      case 'deactivate':
        return 'Confirm deactivation';
      case 'reactivate':
        return 'Confirm reactivation';
      case 'remove':
        return 'Permanently remove';
    }
  }

  private load(showLoading = true): void {
    if (!this.personId) {
      this.errorMessage.set('Person not found.');
      return;
    }
    if (showLoading) this.detail.set(undefined);
    this.api.detail(this.personId).subscribe({
      next: (detail) => {
        this.detail.set(detail);
        this.roleSelection.set(detail.role);
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private successMessage(action: LifecycleAction, person: PersonDetail): string {
    switch (action.kind) {
      case 'role':
        return `Role changed to ${this.roles.find((item) => item.value === action.role)?.label ?? action.role}.`;
      case 'deactivate':
        return 'Account deactivated. Existing sessions were revoked.';
      case 'reactivate':
        return 'Account reactivated. A new sign-in is required.';
      case 'remove':
        return `${person.displayName} was removed. Access cannot be restored; reporting records were retained.`;
    }
  }
}
