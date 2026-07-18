# Author the progression framework — Detailed design

## Overview

Author the progression framework — vertical slice that refines `L2-CURRIC-006` into one independently deliverable interaction. Staff shall define ordered stages and their competencies.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Curriculum Authoring subsystem owns this feature slice.

A program administrator starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Author the progression framework vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `YouthAnnouncements` | Frontend page | [`../../../../frontend/projects/portal/src/app/features/youth/announcements/youth-announcements.ts`](../../../../frontend/projects/portal/src/app/features/youth/announcements/youth-announcements.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `YouthHome` | Frontend page | [`../../../../frontend/projects/portal/src/app/features/youth/home/youth-home.ts`](../../../../frontend/projects/portal/src/app/features/youth/home/youth-home.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `CurriculumAuthoringApi` | Frontend API boundary | Planned boundary | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `ProgressionController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/CurriculumAuthoring/ProgressionController.cs`](../../../../backend/src/WordUp.Api/Controllers/CurriculumAuthoring/ProgressionController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `AddCompetencyCommand` | Application slice | [`../../../../backend/src/WordUp.Application/CurriculumAuthoring/Progression/AddCompetencyCommand.cs`](../../../../backend/src/WordUp.Application/CurriculumAuthoring/Progression/AddCompetencyCommand.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `AddCompetencyCommandHandler` | Application slice | [`../../../../backend/src/WordUp.Application/CurriculumAuthoring/Progression/AddCompetencyCommandHandler.cs`](../../../../backend/src/WordUp.Application/CurriculumAuthoring/Progression/AddCompetencyCommandHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `Competency` | Domain state | [`../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Competency.cs`](../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Competency.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `ProgressionStage` | Domain state | [`../../../../backend/src/WordUp.Domain/CurriculumAuthoring/ProgressionStage.cs`](../../../../backend/src/WordUp.Domain/CurriculumAuthoring/ProgressionStage.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `Assignment` | Domain state | [`../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Assignment.cs`](../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Assignment.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `Badge` | Domain state | [`../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Badge.cs`](../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Badge.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `IAppDbContext` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Abstractions/IAppDbContext.cs`](../../../../backend/src/WordUp.Application/Shared/Abstractions/IAppDbContext.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

- `YouthAnnouncements.submit()` starts the browser interaction and maintains presentation state.

- `CurriculumAuthoringApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `ProgressionController.PostAsync()` applies transport policy and constructs `AddCompetencyCommand`.

- `AddCompetencyCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `Competency`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. A program administrator starts this feature interaction through the Portal application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-CURRIC-006`, `L2-MENTOR-005`, `L2-REPORT-001`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-CURRIC-006` | `L1-CURRIC-005` | Staff shall define ordered stages and their competencies. |
| `L2-MENTOR-005` | `L1-MENTOR-005` | Mentors shall set competency levels per youth with notes. |
| `L2-REPORT-001` | `L1-REPORT-001` | Staff shall access org, youth, cohort, attendance, and staff reports with filters. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Author the progression framework](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Author the progression framework. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Author the progression framework](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Author the progression framework. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Author the progression framework](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Author the progression framework. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Author the progression framework](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Author the progression framework. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Author the progression framework](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Author the progression framework. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Author the progression framework](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Author the progression framework. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
