# Meet the accessibility standard — Detailed design

## Overview

Meet the accessibility standard — vertical slice that refines `L2-NFR-010` into one independently deliverable interaction. The UI shall be operable and perceivable per WCAG 2.1 AA.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Non-Functional and Quality subsystem owns this feature slice.

A Word Up participant starts this feature interaction through the Public Site application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Meet the accessibility standard vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `DisplayPreferencesService` | Frontend page | [`../../../../frontend/projects/portal/src/app/settings/display-preferences.service.ts`](../../../../frontend/projects/portal/src/app/settings/display-preferences.service.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `SettingsDisplay` | Frontend page | [`../../../../frontend/projects/portal/src/app/settings/display/settings-display.ts`](../../../../frontend/projects/portal/src/app/settings/display/settings-display.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `DisplayPreferencesApi` | Frontend API boundary | [`../../../../frontend/projects/wu-api/src/lib/services/display-preferences-api.ts`](../../../../frontend/projects/wu-api/src/lib/services/display-preferences-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `NonFunctionalController` | API transport | Planned boundary | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `MeetAccessibilityStandardCommand` | Application slice | Planned boundary | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `MeetAccessibilityStandardCommandHandler` | Application slice | Planned boundary | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `MeetAccessibilityStandardState` | Domain state | Planned boundary | Holds persistent state and lifecycle invariants used by the feature. |
| `SecurityHeadersMiddleware` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Api/Middleware/SecurityHeadersMiddleware.cs`](../../../../backend/src/WordUp.Api/Middleware/SecurityHeadersMiddleware.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |
| `MaintenanceMiddleware` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Api/Middleware/MaintenanceMiddleware.cs`](../../../../backend/src/WordUp.Api/Middleware/MaintenanceMiddleware.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Public Site application; Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

- `DisplayPreferencesService.submit()` starts the browser interaction and maintains presentation state.

- `DisplayPreferencesApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `NonFunctionalController.PostAsync()` applies transport policy and constructs `MeetAccessibilityStandardCommand`.

- `MeetAccessibilityStandardCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `MeetAccessibilityStandardState`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. A Word Up participant starts this feature interaction through the Public Site application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-NFR-010`, `L2-PLATFORM-004`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-NFR-010` | `L1-NFR-007` | The UI shall be operable and perceivable per WCAG 2.1 AA. |
| `L2-PLATFORM-004` | `L1-PLATFORM-002` | Users shall set text size, high contrast, reduced motion, underline links, and reading font. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Meet the accessibility standard](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Meet the accessibility standard. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Meet the accessibility standard](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Meet the accessibility standard. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Meet the accessibility standard](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Meet the accessibility standard. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Meet the accessibility standard](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Meet the accessibility standard. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Meet the accessibility standard](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Meet the accessibility standard. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Meet the accessibility standard](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Meet the accessibility standard. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
