# Author lessons — Detailed design

## Overview

Author lessons — vertical slice that refines `L2-CURRIC-002` into one independently deliverable interaction. Staff shall author lesson content including teaching text, media, reflection prompts, and leader guides.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Curriculum Authoring subsystem owns this feature slice.

A program administrator starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Author lessons vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `AuthorLessonsPage` | Frontend page | Planned boundary | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `CurriculumAuthoringApi` | Frontend API boundary | Planned boundary | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `LessonsController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/CurriculumAuthoring/LessonsController.cs`](../../../../backend/src/WordUp.Api/Controllers/CurriculumAuthoring/LessonsController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `CreateLessonCommand` | Application slice | [`../../../../backend/src/WordUp.Application/CurriculumAuthoring/Curriculum/CreateLessonCommand.cs`](../../../../backend/src/WordUp.Application/CurriculumAuthoring/Curriculum/CreateLessonCommand.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `CreateLessonCommandHandler` | Application slice | [`../../../../backend/src/WordUp.Application/CurriculumAuthoring/Curriculum/CreateLessonCommandHandler.cs`](../../../../backend/src/WordUp.Application/CurriculumAuthoring/Curriculum/CreateLessonCommandHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `CreateLessonCommandValidator` | Application slice | [`../../../../backend/src/WordUp.Application/CurriculumAuthoring/Curriculum/CreateLessonCommandValidator.cs`](../../../../backend/src/WordUp.Application/CurriculumAuthoring/Curriculum/CreateLessonCommandValidator.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `Lesson` | Domain state | [`../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Lesson.cs`](../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Lesson.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `Module` | Domain state | [`../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Module.cs`](../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Module.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `PublishStatus` | Domain state | [`../../../../backend/src/WordUp.Domain/CurriculumAuthoring/PublishStatus.cs`](../../../../backend/src/WordUp.Domain/CurriculumAuthoring/PublishStatus.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `LessonMedia` | Domain state | [`../../../../backend/src/WordUp.Domain/CurriculumAuthoring/LessonMedia.cs`](../../../../backend/src/WordUp.Domain/CurriculumAuthoring/LessonMedia.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `SecurityHeadersMiddleware` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Api/Middleware/SecurityHeadersMiddleware.cs`](../../../../backend/src/WordUp.Api/Middleware/SecurityHeadersMiddleware.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |
| `ValidationBehavior` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Behaviors/ValidationBehavior.cs`](../../../../backend/src/WordUp.Application/Shared/Behaviors/ValidationBehavior.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

- `AuthorLessonsPage.submit()` starts the browser interaction and maintains presentation state.

- `CurriculumAuthoringApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `LessonsController.PostAsync()` applies transport policy and constructs `CreateLessonCommand`.

- `CreateLessonCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `Lesson`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. A program administrator starts this feature interaction through the Portal application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-CURRIC-002`, `L2-NFR-001`, `L2-CONSENT-003`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-CURRIC-002` | `L1-CURRIC-001` | Staff shall author lesson content including teaching text, media, reflection prompts, and leader guides. |
| `L2-NFR-001` | `L1-NFR-001` | All input shall be validated and all output encoded to prevent injection and XSS. |
| `L2-CONSENT-003` | `L1-CONSENT-002` | The system shall record an append-only audit trail with actor, action, target entity, timestamp, and source. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Author lessons](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Author lessons. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Author lessons](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Author lessons. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Author lessons](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Author lessons. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Author lessons](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Author lessons. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Author lessons](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Author lessons. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Author lessons](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Author lessons. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
