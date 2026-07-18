# Submit an application — Detailed design

## Overview

Submit an application — vertical slice that refines `L2-INTAKE-001` into one independently deliverable interaction. The application form shall capture who is filling it out (self / guardian / referrer), youth details, guardian contact, neighbourhood, cohort of interest, track interest, motivation, and consent/privacy acknowledgements, and shall enforce eligibility and under-18 guardian rules.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Application Intake subsystem owns this feature slice.

An applicant or referrer starts this feature interaction through the Public Site application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Submit an application vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `ApplicationsList` | Frontend page | [`../../../../frontend/projects/portal/src/app/features/admin/applications/applications-list.ts`](../../../../frontend/projects/portal/src/app/features/admin/applications/applications-list.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `ConsentRequired` | Frontend page | [`../../../../frontend/projects/portal/src/app/states/consent-required/consent-required.ts`](../../../../frontend/projects/portal/src/app/states/consent-required/consent-required.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `ApplicationsApi` | Frontend API boundary | [`../../../../frontend/projects/wu-api/src/lib/services/applications-api.ts`](../../../../frontend/projects/wu-api/src/lib/services/applications-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `ApplicationsController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/ApplicationIntake/ApplicationsController.cs`](../../../../backend/src/WordUp.Api/Controllers/ApplicationIntake/ApplicationsController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `SubmitApplicationCommand` | Application slice | [`../../../../backend/src/WordUp.Application/ApplicationIntake/SubmitApplicationCommand.cs`](../../../../backend/src/WordUp.Application/ApplicationIntake/SubmitApplicationCommand.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `SubmitApplicationCommandHandler` | Application slice | [`../../../../backend/src/WordUp.Application/ApplicationIntake/SubmitApplicationCommandHandler.cs`](../../../../backend/src/WordUp.Application/ApplicationIntake/SubmitApplicationCommandHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `SubmitApplicationCommandValidator` | Application slice | [`../../../../backend/src/WordUp.Application/ApplicationIntake/SubmitApplicationCommandValidator.cs`](../../../../backend/src/WordUp.Application/ApplicationIntake/SubmitApplicationCommandValidator.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `SubmitApplicationResult` | Application slice | [`../../../../backend/src/WordUp.Application/ApplicationIntake/SubmitApplicationResult.cs`](../../../../backend/src/WordUp.Application/ApplicationIntake/SubmitApplicationResult.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `YouthApplication` | Domain state | [`../../../../backend/src/WordUp.Domain/ApplicationIntake/YouthApplication.cs`](../../../../backend/src/WordUp.Domain/ApplicationIntake/YouthApplication.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `ApplicationStatus` | Domain state | [`../../../../backend/src/WordUp.Domain/ApplicationIntake/ApplicationStatus.cs`](../../../../backend/src/WordUp.Domain/ApplicationIntake/ApplicationStatus.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `ValidationBehavior` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Behaviors/ValidationBehavior.cs`](../../../../backend/src/WordUp.Application/Shared/Behaviors/ValidationBehavior.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |
| `AppException` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Common/AppException.cs`](../../../../backend/src/WordUp.Application/Shared/Common/AppException.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |
| `ConflictException` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Common/ConflictException.cs`](../../../../backend/src/WordUp.Application/Shared/Common/ConflictException.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Public Site application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

- `ApplicationsList.submit()` starts the browser interaction and maintains presentation state.

- `ApplicationsApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `ApplicationsController.PostAsync()` applies transport policy and constructs `SubmitApplicationCommand`.

- `SubmitApplicationCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `YouthApplication`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. An applicant or referrer starts this feature interaction through the Public Site application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-INTAKE-001`, `L2-NFR-001`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-INTAKE-001` | `L1-INTAKE-001` | The application form shall capture who is filling it out (self / guardian / referrer), youth details, guardian contact, neighbourhood, cohort of interest, track interest, motivation, and consent/privacy acknowledgements, and shall enforce eligibility and under-18 guardian rules. |
| `L2-NFR-001` | `L1-NFR-001` | All input shall be validated and all output encoded to prevent injection and XSS. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Submit an application](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Submit an application. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Submit an application](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Submit an application. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Submit an application](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Submit an application. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Submit an application](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Submit an application. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Submit an application](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Submit an application. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Submit an application](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Submit an application. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
