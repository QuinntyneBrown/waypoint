# Edit a profile

## Overview

Waypoint is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Platform Services subsystem owns this feature slice.

The Edit a profile feature slice refines `L2-PLATFORM-002` into one independently deliverable interaction. Users shall manage profile details, photo, and password.

An authenticated participant starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The Waypoint software system is the entity of interest (EoI). This focused architecture description (AD) addresses the feature boundary, participating applications, software structure, requirements, and behavior. It does not claim conformance as a complete AD; stakeholder perspectives, complete concern coverage, correspondences outside requirement traceability, and architecture decisions remain in the system-level AD.

## Description

### Vertical slice boundary

The feature spans Portal application; Waypoint API application. The frontend application owns interaction state and presentation. The Waypoint API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Change a password is an adjacent feature slice that owns other outcomes within the same primary L2 capability.

### Components and types

A source link identifies an existing realization. An unlinked name identifies a planned feature boundary where the repository has no dedicated type.

| Layer | Components, types, and classes | Responsibility |
| --- | --- | --- |
| Frontend page | [`SettingsProfile`](../../../../frontend/projects/account/src/lib/settings/profile/settings-profile.ts)<br>[`DisplayPreferencesService`](../../../../frontend/projects/account/src/lib/settings/display-preferences.service.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| Frontend API boundary | [`ProfileApi`](../../../../frontend/projects/api/src/lib/services/profile-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| API transport | [`ProfileController`](../../../../backend/src/Waypoint.Api/Controllers/PlatformServices/ProfileController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| Application slice | [`UpdateProfileCommand`](../../../../backend/src/Waypoint.Application/PlatformServices/Profile/UpdateProfileCommand.cs)<br>[`UpdateProfileCommandHandler`](../../../../backend/src/Waypoint.Application/PlatformServices/Profile/UpdateProfileCommandHandler.cs)<br>[`UpdateProfileCommandValidator`](../../../../backend/src/Waypoint.Application/PlatformServices/Profile/UpdateProfileCommandValidator.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| Domain state | [`SupportRequest`](../../../../backend/src/Waypoint.Domain/PlatformServices/SupportRequest.cs)<br>[`DisplayPreference`](../../../../backend/src/Waypoint.Domain/PlatformServices/DisplayPreference.cs)<br>[`NotificationPreference`](../../../../backend/src/Waypoint.Domain/PlatformServices/NotificationPreference.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| Infrastructure and shared ports | [`ValidationBehavior`](../../../../backend/src/Waypoint.Application/Shared/Behaviors/ValidationBehavior.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Functions and data flow

- `SettingsProfile.submit()` starts the browser interaction and maintains presentation state.
- `ProfileApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.
- `ProfileController.PostAsync()` applies transport policy and constructs `UpdateProfileCommand`.
- `UpdateProfileCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `SupportRequest`, and returns a typed result.
- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behavior

1. **Entry.** An authenticated participant starts this feature interaction through the Portal application.
2. **Processing.** The frontend application captures the interaction and sends a typed request to the Waypoint API application. The controller applies endpoint policy. The application handler applies `L2-PLATFORM-002`, `L2-NFR-001`, `L2-AUTH-010`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.
3. **Result.** The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 specification text and L1 trace identifiers. It contains no L1 specification text. Normative wording uses `shall` in accordance with the architecture-description voice.

### L2-PLATFORM-002 — Profile settings

| Field | Value |
| --- | --- |
| Relationship | Primary |
| L1 specification ID | `L1-PLATFORM-002` |

Users shall manage profile details, photo, and password.

### L2-NFR-001 — Input validation and injection defense

| Field | Value |
| --- | --- |
| Relationship | Dependency |
| L1 specification ID | `L1-NFR-001` |

All input shall be validated and all output encoded to prevent injection and XSS.

### L2-AUTH-010 — Role-scoped data visibility

| Field | Value |
| --- | --- |
| Relationship | Dependency |
| L1 specification ID | `L1-AUTH-007` |

Each role shall see only the data its scope permits, per the role scopes defined by the PRD (see L2-AUTH-009).

## Diagrams

The six figures use one visual vocabulary. Blue boxes identify frontend elements, green boxes identify backend application elements, purple boxes identify domain or data elements, and orange boxes identify infrastructure or external elements. Nested boxes identify the software boundary, the frontend or backend boundary, and the owning application.

### C4 diagrams

#### System context

Figure 1 is a view component governed by the C4 system-context model kind. It identifies the people and external software systems that interact with the Waypoint software system for this feature.

![Figure 1 — C4 system context for Edit a profile.](diagrams/c4-system-context.png)

*Figure 1 — C4 system context: Edit a profile in the Waypoint environment.*

#### Container

Figure 2 is a view component governed by the C4 container model kind. It separates frontend containers from backend and persistence containers.

![Figure 2 — C4 container view for Edit a profile.](diagrams/c4-container.png)

*Figure 2 — C4 container view: application and data boundaries for Edit a profile.*

#### Component

Figure 3 is a view component governed by the C4 component model kind. It locates the page, typed client, controller, handler, domain type, and persistence adapter inside their owning applications.

![Figure 3 — C4 component view for Edit a profile.](diagrams/c4-component.png)

*Figure 3 — C4 component view: collaborating components for Edit a profile.*

#### Code

Figure 4 is a view component governed by the C4 code model kind. It shows the principal code elements and their dependency direction.

![Figure 4 — C4 code view for Edit a profile.](diagrams/c4-code.png)

*Figure 4 — C4 code view: feature-owned code elements for Edit a profile.*

### Class diagram

Figure 5 is a view component governed by the UML class-structure model kind. Solid relationships show runtime collaboration, dashed relationships show construction, and aggregation shows persistence ownership.

![Figure 5 — Class structure for Edit a profile.](diagrams/class-structure.png)

*Figure 5 — Class structure: relationships among frontend, API, application, domain, and persistence types for Edit a profile.*

### Sequence diagram

Figure 6 is a view component governed by the UML sequence model kind. Frontend and backend boxes identify the application context for each participant.

![Figure 6 — Sequence behavior for Edit a profile.](diagrams/sequence-behaviour.png)

*Figure 6 — Sequence behavior: the end-to-end edit a profile interaction.*
