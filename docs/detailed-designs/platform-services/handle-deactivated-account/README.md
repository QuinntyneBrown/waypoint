# Handle a deactivated account — Detailed design

## Overview

Handle a deactivated account — vertical slice that refines `L2-PLATFORM-010` into one independently deliverable interaction. Expired sessions and deactivated accounts shall present appropriate states.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Platform Services subsystem owns this feature slice.

An authenticated participant starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Handle a deactivated account vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `AccountDeactivated` | Frontend page | [`../../../../frontend/projects/portal/src/app/states/account-deactivated/account-deactivated.ts`](../../../../frontend/projects/portal/src/app/states/account-deactivated/account-deactivated.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `AccountLocked` | Frontend page | [`../../../../frontend/projects/portal/src/app/states/account-locked/account-locked.ts`](../../../../frontend/projects/portal/src/app/states/account-locked/account-locked.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `PasswordResetApi` | Frontend API boundary | [`../../../../frontend/projects/wu-api/src/lib/services/password-reset-api.ts`](../../../../frontend/projects/wu-api/src/lib/services/password-reset-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `SupportController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/PlatformServices/SupportController.cs`](../../../../backend/src/WordUp.Api/Controllers/PlatformServices/SupportController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `HandleDeactivatedAccountCommand` | Application slice | Planned boundary | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `HandleDeactivatedAccountCommandHandler` | Application slice | Planned boundary | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `SupportRequest` | Domain state | [`../../../../backend/src/WordUp.Domain/PlatformServices/SupportRequest.cs`](../../../../backend/src/WordUp.Domain/PlatformServices/SupportRequest.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `DisplayPreference` | Domain state | [`../../../../backend/src/WordUp.Domain/PlatformServices/DisplayPreference.cs`](../../../../backend/src/WordUp.Domain/PlatformServices/DisplayPreference.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `NotificationPreference` | Domain state | [`../../../../backend/src/WordUp.Domain/PlatformServices/NotificationPreference.cs`](../../../../backend/src/WordUp.Domain/PlatformServices/NotificationPreference.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `IAppDbContext` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Abstractions/IAppDbContext.cs`](../../../../backend/src/WordUp.Application/Shared/Abstractions/IAppDbContext.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Recover an expired session is an adjacent feature slice that owns other outcomes within the same primary L2 capability.

- `AccountDeactivated.submit()` starts the browser interaction and maintains presentation state.

- `PasswordResetApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `SupportController.PostAsync()` applies transport policy and constructs `HandleDeactivatedAccountCommand`.

- `HandleDeactivatedAccountCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `SupportRequest`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. An authenticated participant starts this feature interaction through the Portal application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-PLATFORM-010`, `L2-ADMIN-002`, `L2-AUTH-009`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-PLATFORM-010` | `L1-PLATFORM-003` | Expired sessions and deactivated accounts shall present appropriate states. |
| `L2-ADMIN-002` | `L1-ADMIN-001` | Each person shall have a detail record supporting edit, role change, password reset, deactivate, and remove, with correct data-retention semantics. |
| `L2-AUTH-009` | `L1-AUTH-007` | Every non-public endpoint shall verify authentication and role/resource authorization server-side; client-side gating is never sufficient. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Handle a deactivated account](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Handle a deactivated account. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Handle a deactivated account](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Handle a deactivated account. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Handle a deactivated account](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Handle a deactivated account. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Handle a deactivated account](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Handle a deactivated account. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Handle a deactivated account](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Handle a deactivated account. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Handle a deactivated account](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Handle a deactivated account. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
