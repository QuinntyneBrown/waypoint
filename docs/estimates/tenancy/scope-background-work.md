# Estimate — Scope background work

## Basis

The estimate assumes an intermediate ASP.NET Core 9 developer familiar with hosted services and SignalR who does not yet know this codebase. It assumes the [isolate-organization-data](isolate-organization-data.md) slice is complete, so `ITenantContext.BeginScope` and the query filters exist. Hours cover implementation, NUnit integration tests, and review fixes. [Detailed design](../../detailed-designs/tenancy/scope-background-work/README.md)

## Full design

| Work item | Hours |
|-----------|------:|
| `ScheduledNotificationProcessor` claim query across active organizations and per-row scope | 5 |
| `OutboundEmailProcessor` claim query and per-row scope | 3 |
| `SignalRChangePublisher` organization capture and audience predicate; `RealtimeHub` and `CurrentRealtimeUser` organization field | 5 |
| Two-organization tests for reminders, email, and real-time role fan-out | 6 |
| Two-organization tests for CSV/PDF export, bulk import, and file download `404` | 5 |
| Review fixes | 2 |
| **Total** | **26** |

## Minimal scope

The minimal scope keeps the per-row scope in both processors and the organization predicate in `PublishToRoleAsync`. These close the paths that would mix organizations. It keeps the export, import, and file tests to one test each. It still satisfies all of `L2-TENANCY-003` with lighter proof. Total: 19 hours.

## Partial descoping

| Descope | What is lost | Hours saved |
|---------|--------------|------------:|
| Organization predicate on `PublishToUserAsync` and `PublishToCohortAsync` | No second barrier; ids are already unique per organization | 1 |
| Separate export, import, and file test per format | Coverage reduced to one representative test each | 3 |
| Skip-suspended filter in the claim queries; deliver to suspended organizations until the suspension slice lands | `L2-TENANCY-005` criterion 3 waits for the suspension slice | 1 |
