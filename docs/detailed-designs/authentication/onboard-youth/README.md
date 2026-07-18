# Onboard a youth — Detailed design

## Overview

Onboard a youth — vertical slice that refines `L2-AUTH-011` into one independently deliverable interaction. A newly activated youth shall be guided through onboarding with the ability to skip.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Authentication and Access subsystem owns this feature slice.

An account holder starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Onboard a youth vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `YouthNotifications` | Frontend page | [`../../../../frontend/projects/portal/src/app/features/youth/notifications/youth-notifications.ts`](../../../../frontend/projects/portal/src/app/features/youth/notifications/youth-notifications.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `YouthAnnouncements` | Frontend page | [`../../../../frontend/projects/portal/src/app/features/youth/announcements/youth-announcements.ts`](../../../../frontend/projects/portal/src/app/features/youth/announcements/youth-announcements.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `OnboardingApi` | Frontend API boundary | [`../../../../frontend/projects/wu-api/src/lib/services/onboarding-api.ts`](../../../../frontend/projects/wu-api/src/lib/services/onboarding-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `OnboardingController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/Authentication/OnboardingController.cs`](../../../../backend/src/WordUp.Api/Controllers/Authentication/OnboardingController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `SkipOnboardingCommand` | Application slice | [`../../../../backend/src/WordUp.Application/Authentication/Onboarding/SkipOnboardingCommand.cs`](../../../../backend/src/WordUp.Application/Authentication/Onboarding/SkipOnboardingCommand.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `SkipOnboardingCommandHandler` | Application slice | [`../../../../backend/src/WordUp.Application/Authentication/Onboarding/SkipOnboardingCommandHandler.cs`](../../../../backend/src/WordUp.Application/Authentication/Onboarding/SkipOnboardingCommandHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `User` | Domain state | [`../../../../backend/src/WordUp.Domain/Authentication/User.cs`](../../../../backend/src/WordUp.Domain/Authentication/User.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `AppException` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Common/AppException.cs`](../../../../backend/src/WordUp.Application/Shared/Common/AppException.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

- `YouthNotifications.submit()` starts the browser interaction and maintains presentation state.

- `OnboardingApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `OnboardingController.PostAsync()` applies transport policy and constructs `SkipOnboardingCommand`.

- `SkipOnboardingCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `User`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. An account holder starts this feature interaction through the Portal application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-AUTH-011`, `L2-PLATFORM-003`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-AUTH-011` | `L1-AUTH-008` | A newly activated youth shall be guided through onboarding with the ability to skip. |
| `L2-PLATFORM-003` | `L1-PLATFORM-002` | Users shall configure per-event notification preferences, delivered as in-app notifications with email as an optional channel. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Onboard a youth](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Onboard a youth. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Onboard a youth](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Onboard a youth. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Onboard a youth](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Onboard a youth. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Onboard a youth](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Onboard a youth. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Onboard a youth](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Onboard a youth. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Onboard a youth](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Onboard a youth. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
