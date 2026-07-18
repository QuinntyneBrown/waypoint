# Submit a coding project — Detailed design

## Overview

Submit a coding project — vertical slice that refines `L2-LEARN-008` into one independently deliverable interaction. Youth shall complete interactive coding lessons and submit coding projects for mentor review.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Learning Experience subsystem owns this feature slice.

A youth participant starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Submit a coding project vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `YouthAnnouncements` | Frontend page | [`../../../../frontend/projects/portal/src/app/features/youth/announcements/youth-announcements.ts`](../../../../frontend/projects/portal/src/app/features/youth/announcements/youth-announcements.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `YouthHome` | Frontend page | [`../../../../frontend/projects/portal/src/app/features/youth/home/youth-home.ts`](../../../../frontend/projects/portal/src/app/features/youth/home/youth-home.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `AnnouncementsApi` | Frontend API boundary | [`../../../../frontend/projects/wu-api/src/lib/services/engagement-api.ts`](../../../../frontend/projects/wu-api/src/lib/services/engagement-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `YouthCurriculumController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/LearningExperience/YouthCurriculumController.cs`](../../../../backend/src/WordUp.Api/Controllers/LearningExperience/YouthCurriculumController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `SubmitAssignmentCommand` | Application slice | [`../../../../backend/src/WordUp.Application/LearningExperience/Submissions/SubmitAssignmentCommand.cs`](../../../../backend/src/WordUp.Application/LearningExperience/Submissions/SubmitAssignmentCommand.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `SubmitAssignmentCommandHandler` | Application slice | [`../../../../backend/src/WordUp.Application/LearningExperience/Submissions/SubmitAssignmentCommandHandler.cs`](../../../../backend/src/WordUp.Application/LearningExperience/Submissions/SubmitAssignmentCommandHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `SubmitAssignmentCommandValidator` | Application slice | [`../../../../backend/src/WordUp.Application/LearningExperience/Submissions/SubmitAssignmentCommandValidator.cs`](../../../../backend/src/WordUp.Application/LearningExperience/Submissions/SubmitAssignmentCommandValidator.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `Submission` | Domain state | [`../../../../backend/src/WordUp.Domain/LearningExperience/Submission.cs`](../../../../backend/src/WordUp.Domain/LearningExperience/Submission.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `SubmissionStatus` | Domain state | [`../../../../backend/src/WordUp.Domain/LearningExperience/SubmissionStatus.cs`](../../../../backend/src/WordUp.Domain/LearningExperience/SubmissionStatus.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `LessonProgress` | Domain state | [`../../../../backend/src/WordUp.Domain/LearningExperience/LessonProgress.cs`](../../../../backend/src/WordUp.Domain/LearningExperience/LessonProgress.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `LessonReadingState` | Domain state | [`../../../../backend/src/WordUp.Domain/LearningExperience/LessonReadingState.cs`](../../../../backend/src/WordUp.Domain/LearningExperience/LessonReadingState.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `IAppDbContext` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Abstractions/IAppDbContext.cs`](../../../../backend/src/WordUp.Application/Shared/Abstractions/IAppDbContext.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Complete a coding lesson is an adjacent feature slice that owns other outcomes within the same primary L2 capability.

- `YouthAnnouncements.submit()` starts the browser interaction and maintains presentation state.

- `AnnouncementsApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `YouthCurriculumController.PostAsync()` applies transport policy and constructs `SubmitAssignmentCommand`.

- `SubmitAssignmentCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `Submission`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. A youth participant starts this feature interaction through the Portal application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-LEARN-008`, `L2-LEARN-006`, `L2-LEARN-007`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-LEARN-008` | `L1-LEARN-003` | Youth shall complete interactive coding lessons and submit coding projects for mentor review. |
| `L2-LEARN-006` | `L1-LEARN-005` | Youth shall view an assignment brief (rubric, due date) and submit work of the allowed types with an attestation. |
| `L2-LEARN-007` | `L1-LEARN-005` | Youth shall track a submission through Under review → Approved or Needs revision, see mentor feedback, and resubmit on revision. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Submit a coding project](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Submit a coding project. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Submit a coding project](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Submit a coding project. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Submit a coding project](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Submit a coding project. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Submit a coding project](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Submit a coding project. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Submit a coding project](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Submit a coding project. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Submit a coding project](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Submit a coding project. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
