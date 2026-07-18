# Schedule sessions — Detailed design

## Overview

Schedule sessions — vertical slice that refines `L2-ENGAGE-001` into one independently deliverable interaction. Staff shall schedule sessions/events and view them by cohort in calendar views.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Engagement and Operations subsystem owns this feature slice.

A mentor, volunteer, or program administrator starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Schedule sessions vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `VolunteerSchedule` | Frontend page | [`../../../../frontend/projects/portal/src/app/features/volunteer/schedule/volunteer-schedule.ts`](../../../../frontend/projects/portal/src/app/features/volunteer/schedule/volunteer-schedule.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `SessionExpired` | Frontend page | [`../../../../frontend/projects/portal/src/app/states/session-expired/session-expired.ts`](../../../../frontend/projects/portal/src/app/states/session-expired/session-expired.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `EngagementApi` | Frontend API boundary | Planned boundary | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `SessionsController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/Engagement/SessionsController.cs`](../../../../backend/src/WordUp.Api/Controllers/Engagement/SessionsController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `CreateSessionCommand` | Application slice | [`../../../../backend/src/WordUp.Application/Engagement/Sessions/CreateSessionCommand.cs`](../../../../backend/src/WordUp.Application/Engagement/Sessions/CreateSessionCommand.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `CreateSessionCommandHandler` | Application slice | [`../../../../backend/src/WordUp.Application/Engagement/Sessions/CreateSessionCommandHandler.cs`](../../../../backend/src/WordUp.Application/Engagement/Sessions/CreateSessionCommandHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `CreateSessionCommandValidator` | Application slice | [`../../../../backend/src/WordUp.Application/Engagement/Sessions/CreateSessionCommandValidator.cs`](../../../../backend/src/WordUp.Application/Engagement/Sessions/CreateSessionCommandValidator.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `CreateSessionResult` | Application slice | [`../../../../backend/src/WordUp.Application/Engagement/Sessions/CreateSessionCommand.cs`](../../../../backend/src/WordUp.Application/Engagement/Sessions/CreateSessionCommand.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `Session` | Domain state | [`../../../../backend/src/WordUp.Domain/Engagement/Session.cs`](../../../../backend/src/WordUp.Domain/Engagement/Session.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `SessionRsvp` | Domain state | [`../../../../backend/src/WordUp.Domain/Engagement/SessionRsvp.cs`](../../../../backend/src/WordUp.Domain/Engagement/SessionRsvp.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `IAppDbContext` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Abstractions/IAppDbContext.cs`](../../../../backend/src/WordUp.Application/Shared/Abstractions/IAppDbContext.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Browse the program calendar is an adjacent feature slice that owns other outcomes within the same primary L2 capability.

- `VolunteerSchedule.submit()` starts the browser interaction and maintains presentation state.

- `EngagementApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `SessionsController.PostAsync()` applies transport policy and constructs `CreateSessionCommand`.

- `CreateSessionCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `Session`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. A mentor, volunteer, or program administrator starts this feature interaction through the Portal application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-ENGAGE-001`, `L2-ADMIN-003`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-ENGAGE-001` | `L1-ENGAGE-001` | Staff shall schedule sessions/events and view them by cohort in calendar views. |
| `L2-ADMIN-003` | `L1-ADMIN-002` | Admins shall create and edit cohorts and manage their membership. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Schedule sessions](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Schedule sessions. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Schedule sessions](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Schedule sessions. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Schedule sessions](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Schedule sessions. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Schedule sessions](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Schedule sessions. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Schedule sessions](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Schedule sessions. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Schedule sessions](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Schedule sessions. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
