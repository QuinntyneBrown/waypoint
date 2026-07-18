# View an upcoming schedule

## Overview

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Volunteer Portal subsystem owns the part of that software concerned with view an upcoming schedule.

The View an upcoming schedule feature slice refines `L2-VOLUNTEER-002` into one independently deliverable interaction. Volunteers shall view their upcoming schedule.

A volunteer starts the view an upcoming schedule interaction through the Portal application. The frontend application presents an authorized and bounded view an upcoming schedule result.

The Word Up software system is the entity of interest (EoI). This focused architecture description (AD) addresses the feature boundary, participating applications, software structure, requirements, and behavior. It does not claim conformance as a complete AD; stakeholder perspectives, complete concern coverage, correspondences outside requirement traceability, and architecture decisions remain in the system-level AD.

## Description

### Vertical slice boundary

The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

### Components and types

A source link identifies an existing realization. An unlinked name identifies a planned feature boundary where the repository has no dedicated type.

| Layer | Components, types, and classes | Responsibility |
| --- | --- | --- |
| Frontend page | [`VolunteerSchedule`](../../../../frontend/projects/portal/src/app/features/volunteer/schedule/volunteer-schedule.ts)<br>[`VolunteerToday`](../../../../frontend/projects/portal/src/app/features/volunteer/today/volunteer-today.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| Frontend API boundary | [`VolunteerApi`](../../../../frontend/projects/wu-api/src/lib/services/volunteer-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| API transport | [`VolunteerController`](../../../../backend/src/WordUp.Api/Controllers/VolunteerPortal/VolunteerController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| Application slice | [`VolunteerScheduleQuery`](../../../../backend/src/WordUp.Application/VolunteerPortal/VolunteerScheduleQuery.cs)<br>[`VolunteerScheduleQueryHandler`](../../../../backend/src/WordUp.Application/VolunteerPortal/VolunteerScheduleQueryHandler.cs)<br>[`VolunteerScheduleItem`](../../../../backend/src/WordUp.Application/VolunteerPortal/VolunteerScheduleQuery.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| Domain state | `ViewUpcomingScheduleState` *(planned boundary)* | Holds persistent state and lifecycle invariants used by the feature. |
| Infrastructure and shared ports | [`IAppDbContext`](../../../../backend/src/WordUp.Application/Shared/Abstractions/IAppDbContext.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Functions and data flow

- `VolunteerSchedule.load()` starts the browser interaction and maintains presentation state.
- `VolunteerApi.get()` crosses the frontend-to-backend boundary with a typed HTTP request.
- `VolunteerController.GetAsync()` applies transport policy and constructs `VolunteerScheduleQuery`.
- `VolunteerScheduleQueryHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `ViewUpcomingScheduleState`, and returns a typed result.
- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behavior

1. **Entry.** A volunteer starts the view an upcoming schedule interaction through the Portal application.
2. **Processing.** The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-VOLUNTEER-002`, `L2-AUTH-010`, `L2-ENGAGE-001`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.
3. **Result.** The frontend application presents an authorized and bounded view an upcoming schedule result.

## Requirements

This section contains L2 specification text and L1 trace identifiers. It contains no L1 specification text. Normative wording uses `shall` in accordance with the architecture-description voice.

### L2-VOLUNTEER-002 — Volunteer schedule

| Field | Value |
| --- | --- |
| Relationship | Primary |
| L1 specification ID | `L1-VOLUNTEER-001` |

Volunteers shall view their upcoming schedule.

### L2-AUTH-010 — Role-scoped data visibility

| Field | Value |
| --- | --- |
| Relationship | Dependency |
| L1 specification ID | `L1-AUTH-007` |

Each role shall see only the data its scope permits, per the role scopes defined by the PRD (see L2-AUTH-009).

### L2-ENGAGE-001 — Session scheduling and calendar

| Field | Value |
| --- | --- |
| Relationship | Dependency |
| L1 specification ID | `L1-ENGAGE-001` |

Staff shall schedule sessions/events and view them by cohort in calendar views.

## Diagrams

The six figures use one visual vocabulary. Blue boxes identify frontend elements, green boxes identify backend application elements, purple boxes identify domain or data elements, and orange boxes identify infrastructure or external elements. Nested boxes identify the software boundary, the frontend or backend boundary, and the owning application.

### C4 diagrams

#### System context

Figure 1 is a view component governed by the C4 system-context model kind. It identifies the people and external software systems that interact with the Word Up software system for this feature.

![Figure 1 — C4 system context for View an upcoming schedule.](diagrams/c4-system-context.png)

*Figure 1 — C4 system context: View an upcoming schedule in the Word Up environment.*

#### Container

Figure 2 is a view component governed by the C4 container model kind. It separates frontend containers from backend and persistence containers.

![Figure 2 — C4 container view for View an upcoming schedule.](diagrams/c4-container.png)

*Figure 2 — C4 container view: application and data boundaries for View an upcoming schedule.*

#### Component

Figure 3 is a view component governed by the C4 component model kind. It locates the page, typed client, controller, handler, domain type, and persistence adapter inside their owning applications.

![Figure 3 — C4 component view for View an upcoming schedule.](diagrams/c4-component.png)

*Figure 3 — C4 component view: collaborating components for View an upcoming schedule.*

#### Code

Figure 4 is a view component governed by the C4 code model kind. It shows the principal code elements and their dependency direction.

![Figure 4 — C4 code view for View an upcoming schedule.](diagrams/c4-code.png)

*Figure 4 — C4 code view: feature-owned code elements for View an upcoming schedule.*

### Class diagram

Figure 5 is a view component governed by the UML class-structure model kind. Solid relationships show runtime collaboration, dashed relationships show construction, and aggregation shows persistence ownership.

![Figure 5 — Class structure for View an upcoming schedule.](diagrams/class-structure.png)

*Figure 5 — Class structure: relationships among frontend, API, application, domain, and persistence types for View an upcoming schedule.*

### Sequence diagram

Figure 6 is a view component governed by the UML sequence model kind. Frontend and backend boxes identify the application context for each participant.

![Figure 6 — Sequence behavior for View an upcoming schedule.](diagrams/sequence-behaviour.png)

*Figure 6 — Sequence behavior: the end-to-end view an upcoming schedule interaction.*
