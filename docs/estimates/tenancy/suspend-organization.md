# Estimate — Suspend an organization

## Basis

The estimate assumes an intermediate full-stack developer in ASP.NET Core 9 and Angular 21 who does not yet know this codebase. It assumes [isolate-organization-data](isolate-organization-data.md), [scope-background-work](scope-background-work.md), and [provision-organization](provision-organization.md) are complete. The middleware, the active-only claim queries, and the Organizations screen therefore already exist. Hours cover implementation, tests, and review fixes. [Detailed design](../../detailed-designs/tenancy/suspend-organization/README.md)

## Full design

| Work item | Hours |
|-----------|------:|
| `Organization.Suspend()` and `Reactivate()` with idempotent transitions | 1 |
| Suspend and reactivate commands, handlers, endpoints, and audit | 4 |
| Sign-in rejection for suspended organizations | 2 |
| Real-time publisher status revalidation | 2 |
| Row actions, state badge, and `ConfirmSuspendOrganizationDialog` | 5 |
| Backend tests: `403` after suspend, sign-in blocked, workers skip, data unchanged after reactivate | 5 |
| Playwright acceptance tests for suspend and reactivate | 3 |
| Review fixes | 2 |
| **Total** | **24** |

## Minimal scope

The minimal scope keeps the domain transitions, the two endpoints with audit, and the status checks that already exist from earlier slices. It drops the Portal row actions and dialog and exposes suspend and reactivate as CLI commands. It still satisfies `L2-TENANCY-005` criteria 1–4, with the confirm step taken at the CLI prompt. Total: 14 hours.

## Partial descoping

| Descope | What is lost | Hours saved |
|---------|--------------|------------:|
| Real-time publisher status revalidation | Open live connections of a suspended organization may receive events until their token expires | 2 |
| Separate sign-in rejection; rely on the `403` after sign-in | Users can complete sign-in, then see the deactivated state on the first request | 2 |
