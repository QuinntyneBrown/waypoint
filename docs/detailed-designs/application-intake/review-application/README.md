# Review an application — Detailed design

## Overview

Review an application — vertical slice that refines `L2-INTAKE-002` into one independently deliverable interaction. Each application shall move through review states and, on approval, be placed into a cohort with an assigned mentor, creating the youth account/enrollment; declines are soft with an optional note.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Application Intake subsystem owns this feature slice.

A program administrator starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Review an application vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `ApplicationsList` | Frontend page | [`../../../../frontend/projects/portal/src/app/features/admin/applications/applications-list.ts`](../../../../frontend/projects/portal/src/app/features/admin/applications/applications-list.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `AccountDeactivated` | Frontend page | [`../../../../frontend/projects/portal/src/app/states/account-deactivated/account-deactivated.ts`](../../../../frontend/projects/portal/src/app/states/account-deactivated/account-deactivated.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `ApplicationsApi` | Frontend API boundary | [`../../../../frontend/projects/wu-api/src/lib/services/applications-api.ts`](../../../../frontend/projects/wu-api/src/lib/services/applications-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `ApplicationsController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/ApplicationIntake/ApplicationsController.cs`](../../../../backend/src/WordUp.Api/Controllers/ApplicationIntake/ApplicationsController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `DeclineApplicationCommand` | Application slice | [`../../../../backend/src/WordUp.Application/ApplicationIntake/DeclineApplicationCommand.cs`](../../../../backend/src/WordUp.Application/ApplicationIntake/DeclineApplicationCommand.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `DeclineApplicationCommandHandler` | Application slice | [`../../../../backend/src/WordUp.Application/ApplicationIntake/DeclineApplicationCommandHandler.cs`](../../../../backend/src/WordUp.Application/ApplicationIntake/DeclineApplicationCommandHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `YouthApplication` | Domain state | [`../../../../backend/src/WordUp.Domain/ApplicationIntake/YouthApplication.cs`](../../../../backend/src/WordUp.Domain/ApplicationIntake/YouthApplication.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `ApplicationStatus` | Domain state | [`../../../../backend/src/WordUp.Domain/ApplicationIntake/ApplicationStatus.cs`](../../../../backend/src/WordUp.Domain/ApplicationIntake/ApplicationStatus.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `AppException` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Common/AppException.cs`](../../../../backend/src/WordUp.Application/Shared/Common/AppException.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |
| `ConflictException` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Common/ConflictException.cs`](../../../../backend/src/WordUp.Application/Shared/Common/ConflictException.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |
| `ForbiddenException` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Common/ForbiddenException.cs`](../../../../backend/src/WordUp.Application/Shared/Common/ForbiddenException.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Decide and enroll an applicant is an adjacent feature slice that owns other outcomes within the same primary L2 capability.

- `ApplicationsList.submit()` starts the browser interaction and maintains presentation state.

- `ApplicationsApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `ApplicationsController.PostAsync()` applies transport policy and constructs `DeclineApplicationCommand`.

- `DeclineApplicationCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `YouthApplication`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. A program administrator starts this feature interaction through the Portal application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-INTAKE-002`, `L2-CONSENT-002`, `L2-AUTH-009`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-INTAKE-002` | `L1-INTAKE-002` | Each application shall move through review states and, on approval, be placed into a cohort with an assigned mentor, creating the youth account/enrollment; declines are soft with an optional note. |
| `L2-CONSENT-002` | `L1-CONSENT-001` | A minor without participation consent shall be blocked from program participation and routed to obtain consent. |
| `L2-AUTH-009` | `L1-AUTH-007` | Every non-public endpoint shall verify authentication and role/resource authorization server-side; client-side gating is never sufficient. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Review an application](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Review an application. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Review an application](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Review an application. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Review an application](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Review an application. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Review an application](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Review an application. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Review an application](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Review an application. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Review an application](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Review an application. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
