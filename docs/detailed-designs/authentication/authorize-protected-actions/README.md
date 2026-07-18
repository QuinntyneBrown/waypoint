# Authorize protected actions — Detailed design

## Overview

Authorize protected actions — vertical slice that refines `L2-AUTH-009` into one independently deliverable interaction. Every non-public endpoint shall verify authentication and role/resource authorization server-side; client-side gating is never sufficient.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Authentication and Access subsystem owns this feature slice.

A program administrator or account holder starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Authorize protected actions vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `AuthStore` | Frontend page | [`../../../../frontend/projects/wu-auth/src/lib/auth-store.ts`](../../../../frontend/projects/wu-auth/src/lib/auth-store.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `ServerError` | Frontend page | [`../../../../frontend/projects/portal/src/app/states/server-error/server-error.ts`](../../../../frontend/projects/portal/src/app/states/server-error/server-error.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `AuthApi` | Frontend API boundary | [`../../../../frontend/projects/wu-api/src/lib/services/auth-api.ts`](../../../../frontend/projects/wu-api/src/lib/services/auth-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `AuthController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/Authentication/AuthController.cs`](../../../../backend/src/WordUp.Api/Controllers/Authentication/AuthController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `AuthorizeProtectedActionsCommand` | Application slice | Planned boundary | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `AuthorizeProtectedActionsCommandHandler` | Application slice | Planned boundary | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `AuthorizeProtectedActionsState` | Domain state | Planned boundary | Holds persistent state and lifecycle invariants used by the feature. |
| `SecurityHeadersMiddleware` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Api/Middleware/SecurityHeadersMiddleware.cs`](../../../../backend/src/WordUp.Api/Middleware/SecurityHeadersMiddleware.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

- `AuthStore.submit()` starts the browser interaction and maintains presentation state.

- `AuthApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `AuthController.PostAsync()` applies transport policy and constructs `AuthorizeProtectedActionsCommand`.

- `AuthorizeProtectedActionsCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `AuthorizeProtectedActionsState`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. A program administrator or account holder starts this feature interaction through the Portal application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-AUTH-009`, `L2-CONSENT-003`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-AUTH-009` | `L1-AUTH-007` | Every non-public endpoint shall verify authentication and role/resource authorization server-side; client-side gating is never sufficient. |
| `L2-CONSENT-003` | `L1-CONSENT-002` | The system shall record an append-only audit trail with actor, action, target entity, timestamp, and source. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Authorize protected actions](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Authorize protected actions. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Authorize protected actions](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Authorize protected actions. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Authorize protected actions](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Authorize protected actions. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Authorize protected actions](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Authorize protected actions. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Authorize protected actions](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Authorize protected actions. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Authorize protected actions](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Authorize protected actions. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
