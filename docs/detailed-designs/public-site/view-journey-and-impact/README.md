# View journey and impact — Detailed design

## Overview

View journey and impact — vertical slice that refines `L2-PUB-002` into one independently deliverable interaction. The landing/about/programs pages shall present the two tracks (Bible & Leadership, Coding for Kingdom), the mentorship model, the current cohorts, the 4-step journey, and impact statistics.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Public Site subsystem owns this feature slice.

An anonymous visitor starts this feature interaction through the Public Site application. The frontend application presents an authorized and bounded feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the View journey and impact vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `Programs` | Frontend page | [`../../../../frontend/projects/public-site/src/app/features/programs/programs.ts`](../../../../frontend/projects/public-site/src/app/features/programs/programs.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `AuthStore` | Frontend page | [`../../../../frontend/projects/wu-auth/src/lib/auth-store.ts`](../../../../frontend/projects/wu-auth/src/lib/auth-store.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `PublicSiteApi` | Frontend API boundary | [`../../../../frontend/projects/wu-api/src/lib/services/public-site-api.ts`](../../../../frontend/projects/wu-api/src/lib/services/public-site-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `PublicController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/PublicSite/PublicController.cs`](../../../../backend/src/WordUp.Api/Controllers/PublicSite/PublicController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `ViewJourneyAndImpactQuery` | Application slice | Planned boundary | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `ViewJourneyAndImpactQueryHandler` | Application slice | Planned boundary | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `ViewJourneyAndImpactState` | Domain state | Planned boundary | Holds persistent state and lifecycle invariants used by the feature. |
| `IAppDbContext` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Abstractions/IAppDbContext.cs`](../../../../backend/src/WordUp.Application/Shared/Abstractions/IAppDbContext.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Public Site application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Browse current cohorts; Explore programs are adjacent feature slices that own other outcomes within the same primary L2 capability.

- `Programs.load()` starts the browser interaction and maintains presentation state.

- `PublicSiteApi.get()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `PublicController.GetAsync()` applies transport policy and constructs `ViewJourneyAndImpactQuery`.

- `ViewJourneyAndImpactQueryHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `ViewJourneyAndImpactState`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. An anonymous visitor starts this feature interaction through the Public Site application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-PUB-002`, `L2-AUTH-010`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The frontend application presents an authorized and bounded feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-PUB-002` | `L1-PUB-001` | The landing/about/programs pages shall present the two tracks (Bible & Leadership, Coding for Kingdom), the mentorship model, the current cohorts, the 4-step journey, and impact statistics. |
| `L2-AUTH-010` | `L1-AUTH-007` | Each role shall see only the data its scope permits, per the role scopes defined by the PRD (see L2-AUTH-009). |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for View journey and impact](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for View journey and impact. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for View journey and impact](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for View journey and impact. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for View journey and impact](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for View journey and impact. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for View journey and impact](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for View journey and impact. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for View journey and impact](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for View journey and impact. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for View journey and impact](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for View journey and impact. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
