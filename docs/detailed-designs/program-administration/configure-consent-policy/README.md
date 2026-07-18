# Configure consent policy — Detailed design

## Overview

Configure consent policy — vertical slice that refines `L2-ADMIN-006` into one independently deliverable interaction. The director shall configure org identity, terms, tracks, and consent policy.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Program Administration subsystem owns this feature slice.

A program administrator starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Configure consent policy vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `ConsentRequired` | Frontend page | [`../../../../frontend/projects/portal/src/app/states/consent-required/consent-required.ts`](../../../../frontend/projects/portal/src/app/states/consent-required/consent-required.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `SettingsDisplay` | Frontend page | [`../../../../frontend/projects/portal/src/app/settings/display/settings-display.ts`](../../../../frontend/projects/portal/src/app/settings/display/settings-display.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `ConsentApi` | Frontend API boundary | [`../../../../frontend/projects/wu-api/src/lib/services/consent-api.ts`](../../../../frontend/projects/wu-api/src/lib/services/consent-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `OrgSettingsController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/ProgramAdministration/OrgSettingsController.cs`](../../../../backend/src/WordUp.Api/Controllers/ProgramAdministration/OrgSettingsController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `GetOrgSettingsQuery` | Application slice | [`../../../../backend/src/WordUp.Application/ProgramAdministration/Settings/GetOrgSettingsQuery.cs`](../../../../backend/src/WordUp.Application/ProgramAdministration/Settings/GetOrgSettingsQuery.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `GetOrgSettingsQueryHandler` | Application slice | [`../../../../backend/src/WordUp.Application/ProgramAdministration/Settings/GetOrgSettingsQueryHandler.cs`](../../../../backend/src/WordUp.Application/ProgramAdministration/Settings/GetOrgSettingsQueryHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `OrgSettings` | Domain state | [`../../../../backend/src/WordUp.Domain/ProgramAdministration/OrgSettings.cs`](../../../../backend/src/WordUp.Domain/ProgramAdministration/OrgSettings.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `Cohort` | Domain state | [`../../../../backend/src/WordUp.Domain/ProgramAdministration/Cohort.cs`](../../../../backend/src/WordUp.Domain/ProgramAdministration/Cohort.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `CohortMentor` | Domain state | [`../../../../backend/src/WordUp.Domain/ProgramAdministration/CohortMentor.cs`](../../../../backend/src/WordUp.Domain/ProgramAdministration/CohortMentor.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `SecurityHeadersMiddleware` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Api/Middleware/SecurityHeadersMiddleware.cs`](../../../../backend/src/WordUp.Api/Middleware/SecurityHeadersMiddleware.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Configure the organization profile; Manage terms and tracks are adjacent feature slices that own other outcomes within the same primary L2 capability.

- `ConsentRequired.submit()` starts the browser interaction and maintains presentation state.

- `ConsentApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `OrgSettingsController.PostAsync()` applies transport policy and constructs `GetOrgSettingsQuery`.

- `GetOrgSettingsQueryHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `OrgSettings`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. A program administrator starts this feature interaction through the Portal application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-ADMIN-006`, `L2-CONSENT-001`, `L2-CONSENT-002`, `L2-CONSENT-003`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-ADMIN-006` | `L1-ADMIN-004` | The director shall configure org identity, terms, tracks, and consent policy. |
| `L2-CONSENT-001` | `L1-CONSENT-001` | The system shall capture guardian participation consent (required) and media/photo consent (optional, granular) with a typed e-signature record. |
| `L2-CONSENT-002` | `L1-CONSENT-001` | A minor without participation consent shall be blocked from program participation and routed to obtain consent. |
| `L2-CONSENT-003` | `L1-CONSENT-002` | The system shall record an append-only audit trail with actor, action, target entity, timestamp, and source. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Configure consent policy](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Configure consent policy. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Configure consent policy](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Configure consent policy. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Configure consent policy](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Configure consent policy. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Configure consent policy](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Configure consent policy. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Configure consent policy](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Configure consent policy. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Configure consent policy](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Configure consent policy. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
