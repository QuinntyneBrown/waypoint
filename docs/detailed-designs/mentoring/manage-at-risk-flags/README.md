# Manage at-risk flags — Detailed design

## Overview

Manage at-risk flags — vertical slice that refines `L2-MENTOR-007` into one independently deliverable interaction. The system shall surface at-risk signals and let mentors raise/resolve flags visible to the director.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Mentoring and Coaching subsystem owns this feature slice.

A mentor starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Manage at-risk flags vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `Support` | Frontend page | [`../../../../frontend/projects/portal/src/app/support/support.ts`](../../../../frontend/projects/portal/src/app/support/support.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `SupportApi` | Frontend API boundary | [`../../../../frontend/projects/wu-api/src/lib/services/engagement-api.ts`](../../../../frontend/projects/wu-api/src/lib/services/engagement-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `FlagsController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/Mentoring/FlagsController.cs`](../../../../backend/src/WordUp.Api/Controllers/Mentoring/FlagsController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `AtRiskListQuery` | Application slice | [`../../../../backend/src/WordUp.Application/Mentoring/Flags/AtRiskListQuery.cs`](../../../../backend/src/WordUp.Application/Mentoring/Flags/AtRiskListQuery.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `AtRiskListQueryHandler` | Application slice | [`../../../../backend/src/WordUp.Application/Mentoring/Flags/AtRiskListQueryHandler.cs`](../../../../backend/src/WordUp.Application/Mentoring/Flags/AtRiskListQueryHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `Flag` | Domain state | [`../../../../backend/src/WordUp.Domain/Mentoring/Flag.cs`](../../../../backend/src/WordUp.Domain/Mentoring/Flag.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `CheckIn` | Domain state | [`../../../../backend/src/WordUp.Domain/Mentoring/CheckIn.cs`](../../../../backend/src/WordUp.Domain/Mentoring/CheckIn.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `SecurityHeadersMiddleware` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Api/Middleware/SecurityHeadersMiddleware.cs`](../../../../backend/src/WordUp.Api/Middleware/SecurityHeadersMiddleware.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

- `Support.submit()` starts the browser interaction and maintains presentation state.

- `SupportApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `FlagsController.PostAsync()` applies transport policy and constructs `AtRiskListQuery`.

- `AtRiskListQueryHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `Flag`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. A mentor starts this feature interaction through the Portal application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-MENTOR-007`, `L2-CONSENT-003`, `L2-ENGAGE-007`, `L2-MENTOR-006`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-MENTOR-007` | `L1-MENTOR-007` | The system shall surface at-risk signals and let mentors raise/resolve flags visible to the director. |
| `L2-CONSENT-003` | `L1-CONSENT-002` | The system shall record an append-only audit trail with actor, action, target entity, timestamp, and source. |
| `L2-ENGAGE-007` | `L1-ENGAGE-005` | The system shall support 1:1 threaded messaging and mentor group broadcast. |
| `L2-MENTOR-006` | `L1-MENTOR-006` | Mentors shall log check-ins with visibility controls and view a history. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Manage at-risk flags](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Manage at-risk flags. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Manage at-risk flags](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Manage at-risk flags. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Manage at-risk flags](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Manage at-risk flags. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Manage at-risk flags](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Manage at-risk flags. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Manage at-risk flags](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Manage at-risk flags. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Manage at-risk flags](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Manage at-risk flags. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
