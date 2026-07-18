# Author lessons

## Overview

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Curriculum Authoring subsystem owns this feature slice.

The Author lessons feature slice refines `L2-CURRIC-002` into one independently deliverable interaction. Staff shall author lesson content including teaching text, media, reflection prompts, and leader guides.

A program administrator starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The Word Up software system is the entity of interest (EoI). This focused architecture description (AD) addresses the feature boundary, participating applications, software structure, requirements, and behavior. It does not claim conformance as a complete AD; stakeholder perspectives, complete concern coverage, correspondences outside requirement traceability, and architecture decisions remain in the system-level AD.

## Description

### Vertical slice boundary

The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

### Components and types

A source link identifies an existing realization. An unlinked name identifies a planned feature boundary where the repository has no dedicated type.

| Layer | Components, types, and classes | Responsibility |
| --- | --- | --- |
| Frontend page | `AuthorLessonsPage` *(planned boundary)* | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| Frontend API boundary | `CurriculumAuthoringApi` *(planned boundary)* | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| API transport | [`LessonsController`](../../../../backend/src/WordUp.Api/Controllers/CurriculumAuthoring/LessonsController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| Application slice | [`CreateLessonCommand`](../../../../backend/src/WordUp.Application/CurriculumAuthoring/Curriculum/CreateLessonCommand.cs)<br>[`CreateLessonCommandHandler`](../../../../backend/src/WordUp.Application/CurriculumAuthoring/Curriculum/CreateLessonCommandHandler.cs)<br>[`CreateLessonCommandValidator`](../../../../backend/src/WordUp.Application/CurriculumAuthoring/Curriculum/CreateLessonCommandValidator.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| Domain state | [`Lesson`](../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Lesson.cs)<br>[`Module`](../../../../backend/src/WordUp.Domain/CurriculumAuthoring/Module.cs)<br>[`PublishStatus`](../../../../backend/src/WordUp.Domain/CurriculumAuthoring/PublishStatus.cs)<br>[`LessonMedia`](../../../../backend/src/WordUp.Domain/CurriculumAuthoring/LessonMedia.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| Infrastructure and shared ports | [`SecurityHeadersMiddleware`](../../../../backend/src/WordUp.Api/Middleware/SecurityHeadersMiddleware.cs)<br>[`ValidationBehavior`](../../../../backend/src/WordUp.Application/Shared/Behaviors/ValidationBehavior.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Functions and data flow

- `AuthorLessonsPage.submit()` starts the browser interaction and maintains presentation state.
- `CurriculumAuthoringApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.
- `LessonsController.PostAsync()` applies transport policy and constructs `CreateLessonCommand`.
- `CreateLessonCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `Lesson`, and returns a typed result.
- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behavior

1. **Entry.** A program administrator starts this feature interaction through the Portal application.
2. **Processing.** The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-CURRIC-002`, `L2-NFR-001`, `L2-CONSENT-003`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.
3. **Result.** The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 specification text and L1 trace identifiers. It contains no L1 specification text. Normative wording uses `shall` in accordance with the architecture-description voice.

### L2-CURRIC-002 — Lessons authoring

| Field | Value |
| --- | --- |
| Relationship | Primary |
| L1 specification ID | `L1-CURRIC-001` |

Staff shall author lesson content including teaching text, media, reflection prompts, and leader guides.

### L2-NFR-001 — Input validation and injection defense

| Field | Value |
| --- | --- |
| Relationship | Dependency |
| L1 specification ID | `L1-NFR-001` |

All input shall be validated and all output encoded to prevent injection and XSS.

### L2-CONSENT-003 — Audit log of security- and safeguarding-relevant actions

| Field | Value |
| --- | --- |
| Relationship | Dependency |
| L1 specification ID | `L1-CONSENT-002` |

The system shall record an append-only audit trail with actor, action, target entity, timestamp, and source.

## Diagrams

The six figures use one visual vocabulary. Blue boxes identify frontend elements, green boxes identify backend application elements, purple boxes identify domain or data elements, and orange boxes identify infrastructure or external elements. Nested boxes identify the software boundary, the frontend or backend boundary, and the owning application.

### C4 diagrams

#### System context

Figure 1 is a view component governed by the C4 system-context model kind. It identifies the people and external software systems that interact with the Word Up software system for this feature.

![Figure 1 — C4 system context for Author lessons.](diagrams/c4-system-context.png)

*Figure 1 — C4 system context: Author lessons in the Word Up environment.*

#### Container

Figure 2 is a view component governed by the C4 container model kind. It separates frontend containers from backend and persistence containers.

![Figure 2 — C4 container view for Author lessons.](diagrams/c4-container.png)

*Figure 2 — C4 container view: application and data boundaries for Author lessons.*

#### Component

Figure 3 is a view component governed by the C4 component model kind. It locates the page, typed client, controller, handler, domain type, and persistence adapter inside their owning applications.

![Figure 3 — C4 component view for Author lessons.](diagrams/c4-component.png)

*Figure 3 — C4 component view: collaborating components for Author lessons.*

#### Code

Figure 4 is a view component governed by the C4 code model kind. It shows the principal code elements and their dependency direction.

![Figure 4 — C4 code view for Author lessons.](diagrams/c4-code.png)

*Figure 4 — C4 code view: feature-owned code elements for Author lessons.*

### Class diagram

Figure 5 is a view component governed by the UML class-structure model kind. Solid relationships show runtime collaboration, dashed relationships show construction, and aggregation shows persistence ownership.

![Figure 5 — Class structure for Author lessons.](diagrams/class-structure.png)

*Figure 5 — Class structure: relationships among frontend, API, application, domain, and persistence types for Author lessons.*

### Sequence diagram

Figure 6 is a view component governed by the UML sequence model kind. Frontend and backend boxes identify the application context for each participant.

![Figure 6 — Sequence behavior for Author lessons.](diagrams/sequence-behaviour.png)

*Figure 6 — Sequence behavior: the end-to-end author lessons interaction.*
