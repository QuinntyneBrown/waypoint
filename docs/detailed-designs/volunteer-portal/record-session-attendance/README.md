# Record session attendance — Detailed design

## Overview

Record session attendance — vertical slice that refines `L2-VOLUNTEER-001` into one independently deliverable interaction. Volunteers shall see only their assigned session and take Present/Absent attendance.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Volunteer Portal subsystem owns this feature slice.

A volunteer starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Record session attendance vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `SessionPersistence` | Frontend page | [`../../../../frontend/projects/wu-auth/src/lib/session-persistence.ts`](../../../../frontend/projects/wu-auth/src/lib/session-persistence.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `SessionStore` | Frontend page | [`../../../../frontend/projects/wu-auth/src/lib/session-store.ts`](../../../../frontend/projects/wu-auth/src/lib/session-store.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `VolunteerApi` | Frontend API boundary | [`../../../../frontend/projects/wu-api/src/lib/services/volunteer-api.ts`](../../../../frontend/projects/wu-api/src/lib/services/volunteer-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `VolunteerController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/VolunteerPortal/VolunteerController.cs`](../../../../backend/src/WordUp.Api/Controllers/VolunteerPortal/VolunteerController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `VolunteerTodayQuery` | Application slice | [`../../../../backend/src/WordUp.Application/VolunteerPortal/VolunteerTodayQuery.cs`](../../../../backend/src/WordUp.Application/VolunteerPortal/VolunteerTodayQuery.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `VolunteerTodayQueryHandler` | Application slice | [`../../../../backend/src/WordUp.Application/VolunteerPortal/VolunteerTodayQueryHandler.cs`](../../../../backend/src/WordUp.Application/VolunteerPortal/VolunteerTodayQueryHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `VolunteerTodayView` | Application slice | [`../../../../backend/src/WordUp.Application/VolunteerPortal/VolunteerTodayQuery.cs`](../../../../backend/src/WordUp.Application/VolunteerPortal/VolunteerTodayQuery.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `RecordSessionAttendanceState` | Domain state | Planned boundary | Holds persistent state and lifecycle invariants used by the feature. |
| `ForbiddenException` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Common/ForbiddenException.cs`](../../../../backend/src/WordUp.Application/Shared/Common/ForbiddenException.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. View today's session is an adjacent feature slice that owns other outcomes within the same primary L2 capability.

- `SessionPersistence.submit()` starts the browser interaction and maintains presentation state.

- `VolunteerApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `VolunteerController.PostAsync()` applies transport policy and constructs `VolunteerTodayQuery`.

- `VolunteerTodayQueryHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `RecordSessionAttendanceState`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. A volunteer starts this feature interaction through the Portal application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-VOLUNTEER-001`, `L2-NFR-004`, `L2-AUTH-010`, `L2-PLATFORM-006`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-VOLUNTEER-001` | `L1-VOLUNTEER-001` | Volunteers shall see only their assigned session and take Present/Absent attendance. |
| `L2-NFR-004` | `L1-NFR-003` | Offline attendance writes shall queue and sync without loss or duplication. |
| `L2-AUTH-010` | `L1-AUTH-007` | Each role shall see only the data its scope permits, per the role scopes defined by the PRD (see L2-AUTH-009). |
| `L2-PLATFORM-006` | `L1-PLATFORM-003` | Unauthorized access shall yield a clear 403 with recovery. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Record session attendance](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Record session attendance. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Record session attendance](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Record session attendance. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Record session attendance](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Record session attendance. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Record session attendance](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Record session attendance. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Record session attendance](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Record session attendance. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Record session attendance](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Record session attendance. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
