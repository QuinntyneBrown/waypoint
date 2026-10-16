# Manage an account lifecycle

## Overview

Waypoint is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Program Administration subsystem owns this feature slice.

The Manage an account lifecycle feature slice refines `L2-ADMIN-002` into one independently deliverable interaction. Each person shall have a detail record supporting edit, role change, password reset, deactivate, and remove, with correct data-retention semantics.

A program administrator starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The Waypoint software system is the entity of interest (EoI). This focused architecture description (AD) addresses the feature boundary, participating applications, software structure, requirements, and behavior. It does not claim conformance as a complete AD; stakeholder perspectives, complete concern coverage, correspondences outside requirement traceability, and architecture decisions remain in the system-level AD.

## Description

### Vertical slice boundary

The feature spans Portal application; Waypoint API application. The frontend application owns interaction state and presentation. The Waypoint API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. View a person record is an adjacent feature slice that owns other outcomes within the same primary L2 capability.

### Components and types

A source link identifies an existing realization. An unlinked name identifies a planned feature boundary where the repository has no dedicated type.

| Layer | Components, types, and classes | Responsibility |
| --- | --- | --- |
| Frontend page | [`AccountDeactivated`](../../../../frontend/projects/account/src/lib/states/account-deactivated/account-deactivated.ts)<br>[`AccountLocked`](../../../../frontend/projects/account/src/lib/states/account-locked/account-locked.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| Frontend API boundary | [`PasswordResetService`](../../../../frontend/projects/api/src/lib/services/password-reset.service.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| API transport | [`OrgSettingsController`](../../../../backend/src/Waypoint.Api/Controllers/ProgramAdministration/OrgSettingsController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| Application slice | [`SendPasswordResetForPersonCommand`](../../../../backend/src/Waypoint.Application/ProgramAdministration/People/SendPasswordResetForPersonCommand.cs)<br>[`SendPasswordResetForPersonCommandHandler`](../../../../backend/src/Waypoint.Application/ProgramAdministration/People/SendPasswordResetForPersonCommandHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| Domain state | [`Cohort`](../../../../backend/src/Waypoint.Domain/ProgramAdministration/Cohort.cs)<br>[`CohortMentor`](../../../../backend/src/Waypoint.Domain/ProgramAdministration/CohortMentor.cs)<br>[`OrgSettings`](../../../../backend/src/Waypoint.Domain/ProgramAdministration/OrgSettings.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| Infrastructure and shared ports | [`SecurityHeadersMiddleware`](../../../../backend/src/Waypoint.Api/Middleware/SecurityHeadersMiddleware.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Functions and data flow

- `AccountDeactivated.submit()` starts the browser interaction and maintains presentation state.
- `PasswordResetService.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.
- `OrgSettingsController.PostAsync()` applies transport policy and constructs `SendPasswordResetForPersonCommand`.
- `SendPasswordResetForPersonCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `Cohort`, and returns a typed result.
- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behavior

1. **Entry.** A program administrator starts this feature interaction through the Portal application.
2. **Processing.** The frontend application captures the interaction and sends a typed request to the Waypoint API application. The controller applies endpoint policy. The application handler applies `L2-ADMIN-002`, `L2-AUTH-005`, `L2-AUTH-009`, `L2-CONSENT-003`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.
3. **Result.** The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 specification text and L1 trace identifiers. It contains no L1 specification text. Normative wording uses `shall` in accordance with the architecture-description voice.

### L2-ADMIN-002 — Person record and account lifecycle

| Field | Value |
| --- | --- |
| Relationship | Primary |
| L1 specification ID | `L1-ADMIN-001` |

Each person shall have a detail record supporting edit, role change, password reset, deactivate, and remove, with correct data-retention semantics.

### L2-AUTH-005 — Password reset

| Field | Value |
| --- | --- |
| Relationship | Dependency |
| L1 specification ID | `L1-AUTH-004` |

Users shall be able to request a reset link and set a new password without the system disclosing account existence.

### L2-AUTH-009 — Authorization is enforced server-side on every protected action

| Field | Value |
| --- | --- |
| Relationship | Dependency |
| L1 specification ID | `L1-AUTH-007` |

Every non-public endpoint shall verify authentication and role/resource authorization server-side; client-side gating is never sufficient.

### L2-CONSENT-003 — Audit log of security- and safeguarding-relevant actions

| Field | Value |
| --- | --- |
| Relationship | Dependency |
| L1 specification ID | `L1-CONSENT-002` |

The system shall record an append-only audit trail with actor, action, target entity, timestamp, and source.

## Diagrams

The six figures use one visual vocabulary. Blue boxes identify frontend elements, green boxes identify backend application elements, purple boxes identify domain or data elements, and orange boxes identify infrastructure or external elements. Nested boxes identify the software boundary, the frontend or backend boundary, and the owning application.

### C4 diagrams

#### System context

Figure 1 is a view component governed by the C4 system-context model kind. It identifies the people and external software systems that interact with the Waypoint software system for this feature.

![Figure 1 — C4 system context for Manage an account lifecycle.](diagrams/c4-system-context.png)

*Figure 1 — C4 system context: Manage an account lifecycle in the Waypoint environment.*

#### Container

Figure 2 is a view component governed by the C4 container model kind. It separates frontend containers from backend and persistence containers.

![Figure 2 — C4 container view for Manage an account lifecycle.](diagrams/c4-container.png)

*Figure 2 — C4 container view: application and data boundaries for Manage an account lifecycle.*

#### Component

Figure 3 is a view component governed by the C4 component model kind. It locates the page, typed client, controller, handler, domain type, and persistence adapter inside their owning applications.

![Figure 3 — C4 component view for Manage an account lifecycle.](diagrams/c4-component.png)

*Figure 3 — C4 component view: collaborating components for Manage an account lifecycle.*

#### Code

Figure 4 is a view component governed by the C4 code model kind. It shows the principal code elements and their dependency direction.

![Figure 4 — C4 code view for Manage an account lifecycle.](diagrams/c4-code.png)

*Figure 4 — C4 code view: feature-owned code elements for Manage an account lifecycle.*

### Class diagram

Figure 5 is a view component governed by the UML class-structure model kind. Solid relationships show runtime collaboration, dashed relationships show construction, and aggregation shows persistence ownership.

![Figure 5 — Class structure for Manage an account lifecycle.](diagrams/class-structure.png)

*Figure 5 — Class structure: relationships among frontend, API, application, domain, and persistence types for Manage an account lifecycle.*

### Sequence diagram

Figure 6 is a view component governed by the UML sequence model kind. Frontend and backend boxes identify the application context for each participant.

![Figure 6 — Sequence behavior for Manage an account lifecycle.](diagrams/sequence-behaviour.png)

*Figure 6 — Sequence behavior: the end-to-end manage an account lifecycle interaction.*
