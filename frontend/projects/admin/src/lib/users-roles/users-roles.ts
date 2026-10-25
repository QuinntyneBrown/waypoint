import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiError, PeopleService, PersonSummary, WRoleName } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  Avatar,
  Button,
  FormField,
  Icon,
  Label,
  Select,
  Shell,
  TextField,
  Alert,
  Card,
  CardBody,
  Badge,
  EmptyState,
  EmptyStateIcon,
  EmptyStateTitle,
  EmptyStateText,
  TableWrap,
  Table,
} from '@waypoint/components';

const ROLES: readonly WRoleName[] = [
  'Youth',
  'Mentor',
  'ProgramAdministrator',
  'Guardian',
  'Volunteer',
];

// Traces to: L2-AUTH-009, L2-ADMIN-001, L2-ADMIN-002
@Component({
  selector: 'ph-users-roles',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    RouterLink,
    Avatar,
    Icon,
    Shell,
    Button,
    FormField,
    Label,
    TextField,
    Select,
    Alert,
    Card,
    CardBody,
    Badge,
    EmptyState,
    EmptyStateIcon,
    EmptyStateTitle,
    EmptyStateText,
    TableWrap,
    Table,
  ],
  templateUrl: './users-roles.html',
  styleUrl: './users-roles.css',
})
export class UsersRolesPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(PeopleService);
  protected readonly roles = ROLES;
  protected readonly people = signal<PersonSummary[]>([]);
  protected readonly selectedRoles = signal<Record<string, string>>({});
  protected readonly search = signal('');
  protected readonly roleFilter = signal('');
  protected readonly page = signal(1);
  protected readonly total = signal(0);
  protected readonly loading = signal(true);
  protected readonly savingId = signal<string | null>(null);
  protected readonly confirmId = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly statusMessage = signal<string | null>(null);

  constructor() {
    this.load();
  }

  protected searchChanged(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  protected roleFilterChanged(event: Event): void {
    this.roleFilter.set((event.target as HTMLSelectElement).value);
    this.page.set(1);
    this.load();
  }

  protected selectedRoleChanged(person: PersonSummary, event: Event): void {
    const role = (event.target as HTMLSelectElement).value;
    this.selectedRoles.update((current) => ({ ...current, [person.id]: role }));
    this.confirmId.set(null);
  }

  protected applySearch(): void {
    this.page.set(1);
    this.load();
  }

  protected requestRoleChange(person: PersonSummary): void {
    const role = this.selectedRoles()[person.id] ?? person.role;
    if (role === person.role) return;
    this.confirmId.set(person.id);
  }

  protected changeRole(person: PersonSummary): void {
    const role = this.selectedRoles()[person.id] ?? person.role;
    this.savingId.set(person.id);
    this.errorMessage.set(null);
    this.api.changeRole(person.id, role).subscribe({
      next: () => {
        this.statusMessage.set(`${person.displayName} is now ${role}.`);
        this.confirmId.set(null);
        this.savingId.set(null);
        this.load();
      },
      error: (error: unknown) => {
        this.errorMessage.set(ApiError.from(error).title);
        this.savingId.set(null);
      },
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
    this.api
      .list({
        page: this.page(),
        pageSize: 25,
        role: this.roleFilter(),
        search: this.search(),
      })
      .subscribe({
        next: (result) => {
          this.people.set(result.items);
          this.total.set(result.total);
          this.selectedRoles.set(
            Object.fromEntries(result.items.map((person) => [person.id, person.role])),
          );
          this.loading.set(false);
        },
        error: (error: unknown) => {
          this.errorMessage.set(ApiError.from(error).title);
          this.loading.set(false);
        },
      });
  }
}
