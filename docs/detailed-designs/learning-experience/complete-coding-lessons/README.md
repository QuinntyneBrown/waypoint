# Complete a coding lesson

## Overview

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Learning Experience subsystem owns the part of that software concerned with complete a coding lesson.

The Complete a coding lesson feature slice refines `L2-LEARN-008` into one independently deliverable interaction. Youth shall complete interactive coding lessons and submit coding projects for mentor review.

A youth participant starts the complete a coding lesson interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative complete a coding lesson result.

The Word Up software system is the entity of interest (EoI). This focused architecture description (AD) addresses the feature boundary, participating applications, software structure, requirements, and behavior. It does not claim conformance as a complete AD; stakeholder perspectives, complete concern coverage, correspondences outside requirement traceability, and architecture decisions remain in the system-level AD.

## Description

### Vertical slice boundary

The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Submit a coding project is an adjacent feature slice that owns other outcomes within the same primary L2 capability.

### Components and types

A source link identifies an existing realization. An unlinked name identifies a planned feature boundary where the repository has no dedicated type.

| Layer | Components, types, and classes | Responsibility |
| --- | --- | --- |
| Frontend page | [`YouthAnnouncements`](../../../../frontend/projects/portal/src/app/features/youth/announcements/youth-announcements.ts)<br>[`YouthHome`](../../../../frontend/projects/portal/src/app/features/youth/home/youth-home.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| Frontend API boundary | [`AnnouncementsApi`](../../../../frontend/projects/wu-api/src/lib/services/engagement-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| API transport | [`YouthCurriculumController`](../../../../backend/src/WordUp.Api/Controllers/LearningExperience/YouthCurriculumController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| Application slice | [`CompleteLessonCommand`](../../../../backend/src/WordUp.Application/LearningExperience/YouthCurriculum/CompleteLessonCommand.cs)<br>[`CompleteLessonCommandHandler`](../../../../backend/src/WordUp.Application/LearningExperience/YouthCurriculum/CompleteLessonCommandHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| Domain state | [`BadgeAward`](../../../../backend/src/WordUp.Domain/LearningExperience/BadgeAward.cs)<br>[`LessonProgress`](../../../../backend/src/WordUp.Domain/LearningExperience/LessonProgress.cs)<br>[`ProgressStatus`](../../../../backend/src/WordUp.Domain/LearningExperience/ProgressStatus.cs)<br>[`LessonReadingState`](../../../../backend/src/WordUp.Domain/LearningExperience/LessonReadingState.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| Infrastructure and shared ports | [`IAppDbContext`](../../../../backend/src/WordUp.Application/Shared/Abstractions/IAppDbContext.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Functions and data flow

- `YouthAnnouncements.submit()` starts the browser interaction and maintains presentation state.
- `AnnouncementsApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.
- `YouthCurriculumController.PostAsync()` applies transport policy and constructs `CompleteLessonCommand`.
- `CompleteLessonCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `BadgeAward`, and returns a typed result.
- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behavior

1. **Entry.** A youth participant starts the complete a coding lesson interaction through the Portal application.
2. **Processing.** The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-LEARN-008`, `L2-LEARN-001`, `L2-CURRIC-005`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.
3. **Result.** The backend commits an authorized state change when applicable, and the frontend application presents the authoritative complete a coding lesson result.

## Requirements

This section contains L2 specification text and L1 trace identifiers. It contains no L1 specification text. Normative wording uses `shall` in accordance with the architecture-description voice.

### L2-LEARN-008 — Coding lesson and project delivery

| Field | Value |
| --- | --- |
| Relationship | Primary |
| L1 specification ID | `L1-LEARN-003` |

Youth shall complete interactive coding lessons and submit coding projects for mentor review.

### L2-LEARN-001 — Curriculum browsing and progression gating

| Field | Value |
| --- | --- |
| Relationship | Dependency |
| L1 specification ID | `L1-LEARN-001` |

Youth shall browse their track's modules/lessons with correct gating and unlock rules.

### L2-CURRIC-005 — Badge authoring and awarding rules

| Field | Value |
| --- | --- |
| Relationship | Dependency |
| L1 specification ID | `L1-CURRIC-004` |

Staff shall define badges with auto or manual award rules and optional competency mapping.

## Diagrams

The six figures use one visual vocabulary. Blue boxes identify frontend elements, green boxes identify backend application elements, purple boxes identify domain or data elements, and orange boxes identify infrastructure or external elements. Nested boxes identify the software boundary, the frontend or backend boundary, and the owning application.

### C4 diagrams

#### System context

Figure 1 is a view component governed by the C4 system-context model kind. It identifies the people and external software systems that interact with the Word Up software system for this feature.

![Figure 1 — C4 system context for Complete a coding lesson.](diagrams/c4-system-context.png)

*Figure 1 — C4 system context: Complete a coding lesson in the Word Up environment.*

#### Container

Figure 2 is a view component governed by the C4 container model kind. It separates frontend containers from backend and persistence containers.

![Figure 2 — C4 container view for Complete a coding lesson.](diagrams/c4-container.png)

*Figure 2 — C4 container view: application and data boundaries for Complete a coding lesson.*

#### Component

Figure 3 is a view component governed by the C4 component model kind. It locates the page, typed client, controller, handler, domain type, and persistence adapter inside their owning applications.

![Figure 3 — C4 component view for Complete a coding lesson.](diagrams/c4-component.png)

*Figure 3 — C4 component view: collaborating components for Complete a coding lesson.*

#### Code

Figure 4 is a view component governed by the C4 code model kind. It shows the principal code elements and their dependency direction.

![Figure 4 — C4 code view for Complete a coding lesson.](diagrams/c4-code.png)

*Figure 4 — C4 code view: feature-owned code elements for Complete a coding lesson.*

### Class diagram

Figure 5 is a view component governed by the UML class-structure model kind. Solid relationships show runtime collaboration, dashed relationships show construction, and aggregation shows persistence ownership.

![Figure 5 — Class structure for Complete a coding lesson.](diagrams/class-structure.png)

*Figure 5 — Class structure: relationships among frontend, API, application, domain, and persistence types for Complete a coding lesson.*

### Sequence diagram

Figure 6 is a view component governed by the UML sequence model kind. Frontend and backend boxes identify the application context for each participant.

![Figure 6 — Sequence behavior for Complete a coding lesson.](diagrams/sequence-behaviour.png)

*Figure 6 — Sequence behavior: the end-to-end complete a coding lesson interaction.*
