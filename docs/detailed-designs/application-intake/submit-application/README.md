# Submit an application

## Overview

Waypoint is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Application Intake subsystem owns this feature slice.

The Submit an application feature slice refines `L2-INTAKE-001` into one independently deliverable interaction. The application form shall capture who is filling it out (self / guardian / referrer), youth details, guardian contact, neighbourhood, cohort of interest, track interest, motivation, and consent/privacy acknowledgements, and shall enforce eligibility and under-18 guardian rules.

An applicant or referrer starts this feature interaction through the Public Site application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The Waypoint software system is the entity of interest (EoI). This focused architecture description (AD) addresses the feature boundary, participating applications, software structure, requirements, and behavior. It does not claim conformance as a complete AD; stakeholder perspectives, complete concern coverage, correspondences outside requirement traceability, and architecture decisions remain in the system-level AD.

## Description

### Vertical slice boundary

The feature spans Public Site application; Waypoint API application. The frontend application owns interaction state and presentation. The Waypoint API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

### Components and types

A source link identifies an existing realization. An unlinked name identifies a planned feature boundary where the repository has no dedicated type.

| Layer | Components, types, and classes | Responsibility |
| --- | --- | --- |
| Frontend page | [`ApplicationsList`](../../../../frontend/projects/admin/src/lib/applications/applications-list.ts)<br>[`ConsentRequired`](../../../../frontend/projects/account/src/lib/states/consent-required/consent-required.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| Frontend API boundary | [`ApplicationsApi`](../../../../frontend/projects/api/src/lib/services/applications-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| API transport | [`ApplicationsController`](../../../../backend/src/Waypoint.Api/Controllers/ApplicationIntake/ApplicationsController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| Application slice | [`SubmitApplicationCommand`](../../../../backend/src/Waypoint.Application/ApplicationIntake/SubmitApplicationCommand.cs)<br>[`SubmitApplicationCommandHandler`](../../../../backend/src/Waypoint.Application/ApplicationIntake/SubmitApplicationCommandHandler.cs)<br>[`SubmitApplicationCommandValidator`](../../../../backend/src/Waypoint.Application/ApplicationIntake/SubmitApplicationCommandValidator.cs)<br>[`SubmitApplicationResult`](../../../../backend/src/Waypoint.Application/ApplicationIntake/SubmitApplicationResult.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| Domain state | [`YouthApplication`](../../../../backend/src/Waypoint.Domain/ApplicationIntake/YouthApplication.cs)<br>[`ApplicationStatus`](../../../../backend/src/Waypoint.Domain/ApplicationIntake/ApplicationStatus.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| Infrastructure and shared ports | [`ValidationBehavior`](../../../../backend/src/Waypoint.Application/Shared/Behaviors/ValidationBehavior.cs)<br>[`AppException`](../../../../backend/src/Waypoint.Application/Shared/Common/AppException.cs)<br>[`ConflictException`](../../../../backend/src/Waypoint.Application/Shared/Common/ConflictException.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Functions and data flow

- `ApplicationsList.submit()` starts the browser interaction and maintains presentation state.
- `ApplicationsApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.
- `ApplicationsController.PostAsync()` applies transport policy and constructs `SubmitApplicationCommand`.
- `SubmitApplicationCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `YouthApplication`, and returns a typed result.
- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behavior

1. **Entry.** An applicant or referrer starts this feature interaction through the Public Site application.
2. **Processing.** The frontend application captures the interaction and sends a typed request to the Waypoint API application. The controller applies endpoint policy. The application handler applies `L2-INTAKE-001`, `L2-NFR-001`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.
3. **Result.** The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 specification text and L1 trace identifiers. It contains no L1 specification text. Normative wording uses `shall` in accordance with the architecture-description voice.

### L2-INTAKE-001 — Application/referral form captures applicant, guardian, and consent data

| Field | Value |
| --- | --- |
| Relationship | Primary |
| L1 specification ID | `L1-INTAKE-001` |

The application form shall capture who is filling it out (self / guardian / referrer), youth details, guardian contact, neighbourhood, cohort of interest, track interest, motivation, and consent/privacy acknowledgements, and shall enforce eligibility and under-18 guardian rules.

### L2-NFR-001 — Input validation and injection defense

| Field | Value |
| --- | --- |
| Relationship | Dependency |
| L1 specification ID | `L1-NFR-001` |

All input shall be validated and all output encoded to prevent injection and XSS.

## Diagrams

The six figures use one visual vocabulary. Blue boxes identify frontend elements, green boxes identify backend application elements, purple boxes identify domain or data elements, and orange boxes identify infrastructure or external elements. Nested boxes identify the software boundary, the frontend or backend boundary, and the owning application.

### C4 diagrams

#### System context

Figure 1 is a view component governed by the C4 system-context model kind. It identifies the people and external software systems that interact with the Waypoint software system for this feature.

![Figure 1 — C4 system context for Submit an application.](diagrams/c4-system-context.png)

*Figure 1 — C4 system context: Submit an application in the Waypoint environment.*

#### Container

Figure 2 is a view component governed by the C4 container model kind. It separates frontend containers from backend and persistence containers.

![Figure 2 — C4 container view for Submit an application.](diagrams/c4-container.png)

*Figure 2 — C4 container view: application and data boundaries for Submit an application.*

#### Component

Figure 3 is a view component governed by the C4 component model kind. It locates the page, typed client, controller, handler, domain type, and persistence adapter inside their owning applications.

![Figure 3 — C4 component view for Submit an application.](diagrams/c4-component.png)

*Figure 3 — C4 component view: collaborating components for Submit an application.*

#### Code

Figure 4 is a view component governed by the C4 code model kind. It shows the principal code elements and their dependency direction.

![Figure 4 — C4 code view for Submit an application.](diagrams/c4-code.png)

*Figure 4 — C4 code view: feature-owned code elements for Submit an application.*

### Class diagram

Figure 5 is a view component governed by the UML class-structure model kind. Solid relationships show runtime collaboration, dashed relationships show construction, and aggregation shows persistence ownership.

![Figure 5 — Class structure for Submit an application.](diagrams/class-structure.png)

*Figure 5 — Class structure: relationships among frontend, API, application, domain, and persistence types for Submit an application.*

### Sequence diagram

Figure 6 is a view component governed by the UML sequence model kind. Frontend and backend boxes identify the application context for each participant.

![Figure 6 — Sequence behavior for Submit an application.](diagrams/sequence-behaviour.png)

*Figure 6 — Sequence behavior: the end-to-end submit an application interaction.*
