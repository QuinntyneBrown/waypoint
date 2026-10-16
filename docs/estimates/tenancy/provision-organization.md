# Estimate — Provision an organization

## Basis

The estimate assumes an intermediate full-stack developer in ASP.NET Core 9 and Angular 21 who does not yet know this codebase. It assumes the [isolate-organization-data](isolate-organization-data.md) slice is complete. It also assumes the mocks for the Organizations screen and dialog exist before work starts, as `AGENTS.md` requires. Hours cover implementation, NUnit tests, Playwright acceptance tests with page objects, and review fixes. [Detailed design](../../detailed-designs/tenancy/provision-organization/README.md)

## Full design

| Work item | Hours |
|-----------|------:|
| `PlatformOperator` role, `ManageOrganizations` policy, CLI `--role PlatformOperator` | 3 |
| `OrganizationsController` with list, create, and invite endpoints | 3 |
| `ListOrganizationsQuery` and handler with user count | 2 |
| `CreateOrganizationCommand`, validator, handler, slug unique index, and audit | 5 |
| `InviteFirstAdministratorCommand` and handler reusing `InviteCommand` in scope | 3 |
| Backend tests, including `403` for operators on organization-data endpoints | 5 |
| `OrganizationsService` and route | 2 |
| `OrganizationsPage` with table and card layouts and loading, empty, and error states | 6 |
| `CreateOrganizationDialog` with validation, duplicate-slug, and invitation step | 6 |
| Playwright page objects and acceptance tests, including keyboard and narrow viewport | 6 |
| Review fixes | 3 |
| **Total** | **44** |

## Minimal scope

The minimal scope keeps the backend in full and drops the Portal screen. Organizations are created and first administrators invited through the CLI as two new commands, `org create` and `org invite`. It still satisfies `L2-TENANCY-004` criteria 1–6 through the CLI and all of `L2-TENANCY-006`. It defers `L2-TENANCY-007` and the screen parts of `L2-TENANCY-004`. Total: 24 hours.

## Partial descoping

| Descope | What is lost | Hours saved |
|---------|--------------|------------:|
| Invitation step inside the dialog; invite from a row action instead | One extra click after creation | 2 |
| Stacked-card layout below 576 px; table scrolls horizontally inside its container | Narrow-screen reading is harder; `L2-TENANCY-007` criterion 1 not met | 2 |
| User-count column | Operators cannot see organization size; `L2-TENANCY-006` criterion 2 not met | 1 |
