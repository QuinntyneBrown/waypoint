# Author assignments — Detailed design

## Overview

Author assignments — vertical slice that refines `L2-CURRIC-004` into one independently deliverable interaction. Staff shall define assignments with submission types, a rubric, and due rules.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Curriculum Authoring subsystem owns this feature slice.

A program administrator starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Author assignments vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `AuthorAssignmentsPage` | Frontend page | Planned boundary | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `CurriculumAuthoringApi` | Frontend API boundary | Planned boundary | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `AssignmentsController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/CurriculumAuthoring/AssignmentsController.cs`](../../../../backend/src/WordUp.Api/Controllers/CurriculumAuthoring/AssignmentsController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `CreateAssignmentCommand` | Application slice | [`../../../../backend/src/WordUp.Application/CurriculumAuthoring/Assessments/CreateAssignmentCommand.cs`](../../../../backend/src/WordUp.Application/CurriculumAuthoring/Assessments/CreateAssignmentCommand.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `CreateAssignmentCommandHandler` | Application slice | [`../../../../backend/src/WordUp.Application/CurriculumAuthoring/Assessments/CreateAssignmentCommandHandler.cs`](../../../../backend/src/WordUp.Application/CurriculumAuthoring/Assessments/CreateAssignmentCommandHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `CreateAssignmentCommandValidator` | Application slice | [`../../../../backend/src/WordUp.Application/CurriculumAuthoring/Assessments/CreateAssignmentCommandValidator.cs`](../../../../backend/src/WordUp.Application/CurriculumAuthoring/Assessments/CreateAssignmentCommandValidator.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `Assignment` | Domain state | [`../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Assignment.cs`](../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Assignment.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `RubricCriterion` | Domain state | [`../../../../backend/src/WordUp.Domain/CurriculumAuthoring/RubricCriterion.cs`](../../../../backend/src/WordUp.Domain/CurriculumAuthoring/RubricCriterion.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `Badge` | Domain state | [`../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Badge.cs`](../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Badge.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `Competency` | Domain state | [`../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Competency.cs`](../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Competency.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `IAppDbContext` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Abstractions/IAppDbContext.cs`](../../../../backend/src/WordUp.Application/Shared/Abstractions/IAppDbContext.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

- `AuthorAssignmentsPage.submit()` starts the browser interaction and maintains presentation state.

- `CurriculumAuthoringApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `AssignmentsController.PostAsync()` applies transport policy and constructs `CreateAssignmentCommand`.

- `CreateAssignmentCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `Assignment`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. A program administrator starts this feature interaction through the Portal application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-CURRIC-004`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-CURRIC-004` | `L1-CURRIC-003` | Staff shall define assignments with submission types, a rubric, and due rules. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Author assignments](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Author assignments. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Author assignments](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Author assignments. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Author assignments](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Author assignments. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Author assignments](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Author assignments. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Author assignments](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Author assignments. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Author assignments](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Author assignments. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
