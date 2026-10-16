# Estimate — Isolate organization data

## Basis

The estimate assumes an intermediate ASP.NET Core 9 and EF Core developer who reads documentation for global query filters and SQL Server migration details and does not yet know this codebase. Hours cover implementation, NUnit unit and integration tests, and review fixes. They exclude requirements, design, and mock work. The schema migration for existing data is estimated separately in [migrate-to-default-organization](migrate-to-default-organization.md). `AppDbContext` has 57 `DbSet` types, and up to 56 of them besides `User` become organization-owned; marking each one is mechanical but each composite unique index needs a decision. [Detailed design](../../detailed-designs/tenancy/isolate-organization-data/README.md)

## Full design

| Work item | Hours |
|-----------|------:|
| `Organization`, `OrganizationStatus`, `IOrganizationOwned`; mark 56 entities and add `User.OrganizationId` with check constraint | 8 |
| `ITenantContext` port and `TenantContext` with claim resolution and explicit scope | 4 |
| `org_id` claim in `JwtTokenIssuer`; organization check in `OnTokenValidated` | 3 |
| `AppDbContext` query-filter loop and save-time stamping and immutability guard | 6 |
| Anonymous entry points: email and token lookups with `IgnoreQueryFilters()`, `BeginScope`, and `TenancyOptions` for public submissions | 6 |
| Composite indexes leading with `OrganizationId`; rework of existing unique indexes | 6 |
| `OwnedRecordLookup` and adoption in load-by-id handlers | 10 |
| `OrganizationStatusMiddleware` and interceptor mapping to account-deactivated | 3 |
| Two-organization integration tests across every subsystem's list and by-id endpoints | 16 |
| Performance suite seed of 50 organizations and p95 comparison | 4 |
| Review fixes | 4 |
| **Total** | **70** |

## Minimal scope

The minimal scope keeps the claim, the tenant context, the query filters, save-time stamping, and the composite indexes. It keeps `404` for cross-organization ids because the query filter yields that on its own. It drops `OwnedRecordLookup`, so cross-organization attempts are not audited, and it drops the 50-organization performance run. It still satisfies `L2-TENANCY-001` and most of `L2-TENANCY-002`; it defers acceptance criterion 2's audit entry and `L2-TENANCY-008`. Suspension handling moves to [suspend-organization](suspend-organization.md). Total: 53 hours.

## Partial descoping

| Descope | What is lost | Hours saved |
|---------|--------------|------------:|
| `OwnedRecordLookup` audit of cross-organization ids | Probing for other organizations' ids is not visible in the audit log | 10 |
| 50-organization performance run | No measured proof that scoping keeps p95 budgets | 4 |
| Two-organization tests limited to the 10 highest-risk endpoints instead of all | Lower confidence that no endpoint bypasses the filter | 8 |
