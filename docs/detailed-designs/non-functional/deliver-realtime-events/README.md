# Deliver real-time events

## Overview

Waypoint is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Non-Functional and Quality subsystem owns this feature slice.

The Deliver real-time events feature slice refines `L2-NFR-005` into one independently deliverable interaction. The system shall deliver notifications and messages to authorized, correctly-scoped recipients in near-real-time.

A Waypoint participant starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The Waypoint software system is the entity of interest (EoI). This focused architecture description (AD) addresses the feature boundary, participating applications, software structure, requirements, and behavior. It does not claim conformance as a complete AD; stakeholder perspectives, complete concern coverage, correspondences outside requirement traceability, and architecture decisions remain in the system-level AD.

## Description

### Vertical slice boundary

The feature spans Portal application; Waypoint API application. The frontend application owns interaction state and presentation. The Waypoint API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

### Components and types

A source link identifies an existing realization. An unlinked name identifies a planned feature boundary where the repository has no dedicated type.

| Layer                           | Components, types, and classes                                                                                                                                                                                                                                                                                                                                                                             | Responsibility                                                                                                                                                  |
| ------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Frontend page                   | [`SettingsNotifications`](../../../../frontend/projects/account/src/lib/settings/notifications/settings-notifications.ts)<br>[`YouthNotifications`](../../../../frontend/projects/youth/src/lib/notifications/youth-notifications.ts)                                                                                                                                                       | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states.                                             |
| Frontend API boundary           | [`NotificationPreferencesService`](../../../../frontend/projects/api/src/lib/services/notification-preferences.service.ts)                                                                                                                                                                                                                                                                                      | Serializes requests, deserializes typed results, and maps safe API errors for the page.                                                                         |
| API transport                   | `NonFunctionalController` _(planned boundary)_                                                                                                                                                                                                                                                                                                                                                             | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR.                                          |
| Application slice               | `DeliverRealtimeEventsCommand` _(planned boundary)_<br>`DeliverRealtimeEventsCommandHandler` _(planned boundary)_                                                                                                                                                                                                                                                                                          | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result.                                                     |
| Domain state                    | `DeliverRealtimeEventsState` _(planned boundary)_                                                                                                                                                                                                                                                                                                                                                          | Holds persistent state and lifecycle invariants used by the feature.                                                                                            |
| Infrastructure and shared ports | [`RealtimeConnectionRegistry`](../../../../backend/src/Waypoint.Api/Realtime/RealtimeConnectionRegistry.cs)<br>[`RealtimeHub`](../../../../backend/src/Waypoint.Api/Realtime/RealtimeHub.cs)<br>[`SignalRChangePublisher`](../../../../backend/src/Waypoint.Api/Realtime/SignalRChangePublisher.cs)<br>[`SecurityHeadersMiddleware`](../../../../backend/src/Waypoint.Api/Middleware/SecurityHeadersMiddleware.cs) | Tracks authenticated connections and revalidates current account, session, role, and cohort state before sending directly to authorized connection identifiers. |

### Functions and data flow

- `SettingsNotifications.submit()` starts the browser interaction and maintains presentation state.
- `NotificationPreferencesService.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.
- `NonFunctionalController.PostAsync()` applies transport policy and constructs `DeliverRealtimeEventsCommand`.
- `DeliverRealtimeEventsCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `DeliverRealtimeEventsState`, and returns a typed result.
- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.
- `RealtimeHub.OnConnectedAsync()` registers the authenticated user, session version, and role snapshot and removes that registration when the connection ends.
- `SignalRChangePublisher` loads current user state before each event and sends only to validated connection identifiers; connect-time group membership is not an authorization boundary.

### Behavior

1. **Entry.** A Waypoint participant starts this feature interaction through the Portal application.
2. **Processing.** The frontend application captures the interaction and sends a typed request to the Waypoint API application. The controller applies endpoint policy. The application handler applies `L2-NFR-005`, `L2-AUTH-010`, `L2-ENGAGE-007`, `L2-ENGAGE-008`, scopes the operation to the current actor, and queries or changes domain state through the persistence port. Before each realtime send, the API revalidates the connected user's current status, anonymization state, session version, role, and requested user or cohort audience.
3. **Result.** The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 specification text and L1 trace identifiers. It contains no L1 specification text. Normative wording uses `shall` in accordance with the architecture-description voice.

### L2-NFR-005 — Near-real-time notification and message delivery

| Field               | Value        |
| ------------------- | ------------ |
| Relationship        | Primary      |
| L1 specification ID | `L1-NFR-004` |

The system shall deliver notifications and messages to authorized, correctly-scoped recipients in near-real-time.

### L2-AUTH-010 — Role-scoped data visibility

| Field               | Value         |
| ------------------- | ------------- |
| Relationship        | Dependency    |
| L1 specification ID | `L1-AUTH-007` |

Each role shall see only the data its scope permits, per the role scopes defined by the PRD (see L2-AUTH-009).

### L2-ENGAGE-007 — Messaging

| Field               | Value           |
| ------------------- | --------------- |
| Relationship        | Dependency      |
| L1 specification ID | `L1-ENGAGE-005` |

The system shall support 1:1 threaded messaging and mentor group broadcast.

### L2-ENGAGE-008 — Notifications

| Field               | Value           |
| ------------------- | --------------- |
| Relationship        | Dependency      |
| L1 specification ID | `L1-ENGAGE-006` |

The system shall generate a per-user notification feed with deep links and read state.

## Diagrams

The six figures use one visual vocabulary. Blue boxes identify frontend elements, green boxes identify backend application elements, purple boxes identify domain or data elements, and orange boxes identify infrastructure or external elements. Nested boxes identify the software boundary, the frontend or backend boundary, and the owning application.

### C4 diagrams

#### System context

Figure 1 is a view component governed by the C4 system-context model kind. It identifies the people and external software systems that interact with the Waypoint software system for this feature.

![Figure 1 — C4 system context for Deliver real-time events.](diagrams/c4-system-context.png)

_Figure 1 — C4 system context: Deliver real-time events in the Waypoint environment._

#### Container

Figure 2 is a view component governed by the C4 container model kind. It separates frontend containers from backend and persistence containers.

![Figure 2 — C4 container view for Deliver real-time events.](diagrams/c4-container.png)

_Figure 2 — C4 container view: application and data boundaries for Deliver real-time events._

#### Component

Figure 3 is a view component governed by the C4 component model kind. It locates the page, typed client, controller, handler, domain type, and persistence adapter inside their owning applications.

![Figure 3 — C4 component view for Deliver real-time events.](diagrams/c4-component.png)

_Figure 3 — C4 component view: collaborating components for Deliver real-time events._

#### Code

Figure 4 is a view component governed by the C4 code model kind. It shows the principal code elements and their dependency direction.

![Figure 4 — C4 code view for Deliver real-time events.](diagrams/c4-code.png)

_Figure 4 — C4 code view: feature-owned code elements for Deliver real-time events._

### Class diagram

Figure 5 is a view component governed by the UML class-structure model kind. Solid relationships show runtime collaboration, dashed relationships show construction, and aggregation shows persistence ownership.

![Figure 5 — Class structure for Deliver real-time events.](diagrams/class-structure.png)

_Figure 5 — Class structure: relationships among frontend, API, application, domain, and persistence types for Deliver real-time events._

### Sequence diagram

Figure 6 is a view component governed by the UML sequence model kind. Frontend and backend boxes identify the application context for each participant.

![Figure 6 — Sequence behavior for Deliver real-time events.](diagrams/sequence-behaviour.png)

_Figure 6 — Sequence behavior: the end-to-end deliver real-time events interaction._
