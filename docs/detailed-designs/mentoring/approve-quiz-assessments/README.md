# Approve quiz assessments

## Overview

Waypoint is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Mentoring and Coaching subsystem owns this feature slice.

The Approve quiz assessments feature slice refines `L2-MENTOR-004` into one independently deliverable interaction. Mentors shall approve or override auto-scored quizzes before they count; youth see only approved scores.

A mentor starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The Waypoint software system is the entity of interest (EoI). This focused architecture description (AD) addresses the feature boundary, participating applications, software structure, requirements, and behavior. It does not claim conformance as a complete AD; stakeholder perspectives, complete concern coverage, correspondences outside requirement traceability, and architecture decisions remain in the system-level AD.

## Description

### Vertical slice boundary

The feature spans Portal application; Waypoint API application. The frontend application owns interaction state and presentation. The Waypoint API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

### Components and types

A source link identifies an existing realization. An unlinked name identifies a planned feature boundary where the repository has no dedicated type.

| Layer | Components, types, and classes | Responsibility |
| --- | --- | --- |
| Frontend page | [`YouthAnnouncements`](../../../../frontend/projects/youth/src/lib/announcements/youth-announcements.ts)<br>[`YouthHome`](../../../../frontend/projects/youth/src/lib/home/youth-home.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| Frontend API boundary | `MentoringService` *(planned boundary)* | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| API transport | [`QuizApprovalController`](../../../../backend/src/Waypoint.Api/Controllers/Mentoring/QuizApprovalController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| Application slice | [`ApproveAllQuizAttemptsCommand`](../../../../backend/src/Waypoint.Application/Mentoring/QuizApproval/ApproveAllQuizAttemptsCommand.cs)<br>[`ApproveAllQuizAttemptsCommandHandler`](../../../../backend/src/Waypoint.Application/Mentoring/QuizApproval/ApproveAllQuizAttemptsCommandHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| Domain state | [`CompetencyAssessment`](../../../../backend/src/Waypoint.Domain/Mentoring/CompetencyAssessment.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| Infrastructure and shared ports | [`SecurityHeadersMiddleware`](../../../../backend/src/Waypoint.Api/Middleware/SecurityHeadersMiddleware.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Functions and data flow

- `YouthAnnouncements.submit()` starts the browser interaction and maintains presentation state.
- `MentoringService.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.
- `QuizApprovalController.PostAsync()` applies transport policy and constructs `ApproveAllQuizAttemptsCommand`.
- `ApproveAllQuizAttemptsCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `CompetencyAssessment`, and returns a typed result.
- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behavior

1. **Entry.** A mentor starts this feature interaction through the Portal application.
2. **Processing.** The frontend application captures the interaction and sends a typed request to the Waypoint API application. The controller applies endpoint policy. The application handler applies `L2-MENTOR-004`, `L2-LEARN-005`, `L2-CONSENT-003`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.
3. **Result.** The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 specification text and L1 trace identifiers. It contains no L1 specification text. Normative wording uses `shall` in accordance with the architecture-description voice.

### L2-MENTOR-004 — Quiz assessment approval and override

| Field | Value |
| --- | --- |
| Relationship | Primary |
| L1 specification ID | `L1-MENTOR-004` |

Mentors shall approve or override auto-scored quizzes before they count; youth see only approved scores.

### L2-LEARN-005 — Quiz result and review

| Field | Value |
| --- | --- |
| Relationship | Dependency |
| L1 specification ID | `L1-LEARN-004` |

Youth shall see their score, pass/fail, per-question review, and any earned badge, subject to the quiz's feedback mode.

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

Figure 1 is a view component governed by the C4 system-context model kind. It identifies the people and external software systems that interact with the Waypoint software system for this feature.

![Figure 1 — C4 system context for Approve quiz assessments.](diagrams/c4-system-context.png)

*Figure 1 — C4 system context: Approve quiz assessments in the Waypoint environment.*

#### Container

Figure 2 is a view component governed by the C4 container model kind. It separates frontend containers from backend and persistence containers.

![Figure 2 — C4 container view for Approve quiz assessments.](diagrams/c4-container.png)

*Figure 2 — C4 container view: application and data boundaries for Approve quiz assessments.*

#### Component

Figure 3 is a view component governed by the C4 component model kind. It locates the page, typed client, controller, handler, domain type, and persistence adapter inside their owning applications.

![Figure 3 — C4 component view for Approve quiz assessments.](diagrams/c4-component.png)

*Figure 3 — C4 component view: collaborating components for Approve quiz assessments.*

#### Code

Figure 4 is a view component governed by the C4 code model kind. It shows the principal code elements and their dependency direction.

![Figure 4 — C4 code view for Approve quiz assessments.](diagrams/c4-code.png)

*Figure 4 — C4 code view: feature-owned code elements for Approve quiz assessments.*

### Class diagram

Figure 5 is a view component governed by the UML class-structure model kind. Solid relationships show runtime collaboration, dashed relationships show construction, and aggregation shows persistence ownership.

![Figure 5 — Class structure for Approve quiz assessments.](diagrams/class-structure.png)

*Figure 5 — Class structure: relationships among frontend, API, application, domain, and persistence types for Approve quiz assessments.*

### Sequence diagram

Figure 6 is a view component governed by the UML sequence model kind. Frontend and backend boxes identify the application context for each participant.

![Figure 6 — Sequence behavior for Approve quiz assessments.](diagrams/sequence-behaviour.png)

*Figure 6 — Sequence behavior: the end-to-end approve quiz assessments interaction.*
