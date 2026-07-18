# Operate and observe the system — Detailed design

## Overview

Operate and observe the system — vertical slice that refines `L2-NFR-011` into one independently deliverable interaction. The system shall expose health checks, structured logging, and an admin CLI (operational tooling — an out-of-band ops interface, not product scope).

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Non-Functional and Quality subsystem owns this feature slice.

A platform operator starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Operate and observe the system vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `NotFound` | Frontend page | [`../../../../frontend/projects/portal/src/app/states/not-found/not-found.ts`](../../../../frontend/projects/portal/src/app/states/not-found/not-found.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `NotFound` | Frontend page | [`../../../../frontend/projects/public-site/src/app/features/not-found/not-found.ts`](../../../../frontend/projects/public-site/src/app/features/not-found/not-found.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `NonFunctionalApi` | Frontend API boundary | Planned boundary | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `NonFunctionalController` | API transport | Planned boundary | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `OperateAndObserveSystemCommand` | Application slice | Planned boundary | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `OperateAndObserveSystemCommandHandler` | Application slice | Planned boundary | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `OperateAndObserveSystemState` | Domain state | Planned boundary | Holds persistent state and lifecycle invariants used by the feature. |
| `SecurityHeadersMiddleware` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Api/Middleware/SecurityHeadersMiddleware.cs`](../../../../backend/src/WordUp.Api/Middleware/SecurityHeadersMiddleware.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |
| `MaintenanceMiddleware` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Api/Middleware/MaintenanceMiddleware.cs`](../../../../backend/src/WordUp.Api/Middleware/MaintenanceMiddleware.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |
| `NotFoundException` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Common/NotFoundException.cs`](../../../../backend/src/WordUp.Application/Shared/Common/NotFoundException.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

- `NotFound.submit()` starts the browser interaction and maintains presentation state.

- `NonFunctionalApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `NonFunctionalController.PostAsync()` applies transport policy and constructs `OperateAndObserveSystemCommand`.

- `OperateAndObserveSystemCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `OperateAndObserveSystemState`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. A platform operator starts this feature interaction through the Portal application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-NFR-011`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-NFR-011` | `L1-NFR-008` | The system shall expose health checks, structured logging, and an admin CLI (operational tooling — an out-of-band ops interface, not product scope). |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Operate and observe the system](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Operate and observe the system. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Operate and observe the system](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Operate and observe the system. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Operate and observe the system](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Operate and observe the system. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Operate and observe the system](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Operate and observe the system. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Operate and observe the system](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Operate and observe the system. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Operate and observe the system](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Operate and observe the system. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
