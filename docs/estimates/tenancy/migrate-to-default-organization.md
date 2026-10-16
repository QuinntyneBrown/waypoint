# Estimate — Migrate to a default organization

## Basis

The estimate assumes an intermediate EF Core and SQL Server developer who does not yet know this codebase. It assumes the [isolate-organization-data](isolate-organization-data.md) model changes exist, so EF Core can scaffold the schema part of the migration. The backfill and index steps are hand-written into the scaffold. The 23 existing unique indexes besides `Users.Email` each need a check of whether they become per-organization. Hours cover implementation, integration tests against SQL Server, and review fixes. [Detailed design](../../detailed-designs/tenancy/migrate-to-default-organization/README.md)

## Full design

| Work item | Hours |
|-----------|------:|
| Scaffold `AddMultiTenancy` and reorder into create, insert, add-nullable, backfill, and constrain steps | 6 |
| Default organization insert from `OrgSettings` | 1 |
| Rework of 23 unique indexes and new lookup indexes | 4 |
| Integration test: seed at the previous migration, migrate, compare row counts and ownership | 5 |
| Integration test: sign-in and dashboard parity after migration | 2 |
| Fault-injection rollback test and second-run no-op test | 3 |
| Rehearsal against a copy of production-sized data and timing | 3 |
| Review fixes | 2 |
| **Total** | **26** |

## Minimal scope

The minimal scope keeps the migration in full, the row-count test, and the second-run test. It drops the fault-injection test and relies on EF Core's transaction-per-migration behaviour. It also drops the production-sized rehearsal. It satisfies `L2-TENANCY-009` criteria 1, 2, 3, and 5, and leaves criterion 4 untested. Total: 18 hours.

## Partial descoping

| Descope | What is lost | Hours saved |
|---------|--------------|------------:|
| Production-sized rehearsal | No measured migration duration before release | 3 |
| Fault-injection rollback test | Criterion 4 rests on EF Core behaviour without a direct test | 2 |
| Dashboard parity test | Criterion 3 rests on the row-count test and manual sign-in | 2 |
