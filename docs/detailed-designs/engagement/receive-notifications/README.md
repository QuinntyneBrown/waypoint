# Receive notifications — Detailed design

## Overview

Receive notifications — vertical slice that refines `L2-ENGAGE-008` into one independently deliverable interaction. The system shall generate a per-user notification feed with deep links and read state.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Engagement and Operations subsystem owns this feature slice.

An authenticated participant starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Receive notifications vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `SettingsNotifications` | Frontend page | [`../../../../frontend/projects/portal/src/app/settings/notifications/settings-notifications.ts`](../../../../frontend/projects/portal/src/app/settings/notifications/settings-notifications.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `YouthNotifications` | Frontend page | [`../../../../frontend/projects/portal/src/app/features/youth/notifications/youth-notifications.ts`](../../../../frontend/projects/portal/src/app/features/youth/notifications/youth-notifications.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `NotificationsApi` | Frontend API boundary | [`../../../../frontend/projects/wu-api/src/lib/services/engagement-api.ts`](../../../../frontend/projects/wu-api/src/lib/services/engagement-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `NotificationsController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/Engagement/NotificationsController.cs`](../../../../backend/src/WordUp.Api/Controllers/Engagement/NotificationsController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `MarkAllNotificationsReadCommand` | Application slice | [`../../../../backend/src/WordUp.Application/Engagement/Notifications/MarkAllNotificationsReadCommand.cs`](../../../../backend/src/WordUp.Application/Engagement/Notifications/MarkAllNotificationsReadCommand.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `MarkAllNotificationsReadCommandHandler` | Application slice | [`../../../../backend/src/WordUp.Application/Engagement/Notifications/MarkAllNotificationsReadCommandHandler.cs`](../../../../backend/src/WordUp.Application/Engagement/Notifications/MarkAllNotificationsReadCommandHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `Notification` | Domain state | [`../../../../backend/src/WordUp.Domain/Engagement/Notification.cs`](../../../../backend/src/WordUp.Domain/Engagement/Notification.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `Message` | Domain state | [`../../../../backend/src/WordUp.Domain/Engagement/Message.cs`](../../../../backend/src/WordUp.Domain/Engagement/Message.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `AnnouncementRead` | Domain state | [`../../../../backend/src/WordUp.Domain/Engagement/AnnouncementRead.cs`](../../../../backend/src/WordUp.Domain/Engagement/AnnouncementRead.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `AppException` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Common/AppException.cs`](../../../../backend/src/WordUp.Application/Shared/Common/AppException.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |
| `ICurrentUser` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Abstractions/ICurrentUser.cs`](../../../../backend/src/WordUp.Application/Shared/Abstractions/ICurrentUser.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

- `SettingsNotifications.submit()` starts the browser interaction and maintains presentation state.

- `NotificationsApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `NotificationsController.PostAsync()` applies transport policy and constructs `MarkAllNotificationsReadCommand`.

- `MarkAllNotificationsReadCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `Notification`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. An authenticated participant starts this feature interaction through the Portal application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-ENGAGE-008`, `L2-PLATFORM-003`, `L2-NFR-005`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-ENGAGE-008` | `L1-ENGAGE-006` | The system shall generate a per-user notification feed with deep links and read state. |
| `L2-PLATFORM-003` | `L1-PLATFORM-002` | Users shall configure per-event notification preferences, delivered as in-app notifications with email as an optional channel. |
| `L2-NFR-005` | `L1-NFR-004` | The system shall deliver notifications and messages to authorized, correctly-scoped recipients in near-real-time. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Receive notifications](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Receive notifications. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Receive notifications](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Receive notifications. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Receive notifications](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Receive notifications. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Receive notifications](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Receive notifications. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Receive notifications](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Receive notifications. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Receive notifications](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Receive notifications. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
