# Accept an invitation — Detailed design

## Overview

Accept an invitation — vertical slice that refines `L2-AUTH-004` into one independently deliverable interaction. An invitee shall accept by setting display name + password; staff shall be able to resend and revoke invitations; expired/revoked links shall not work.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Authentication and Access subsystem owns this feature slice.

A program administrator or account holder starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Accept an invitation vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `ConsentRequired` | Frontend page | [`../../../../frontend/projects/portal/src/app/states/consent-required/consent-required.ts`](../../../../frontend/projects/portal/src/app/states/consent-required/consent-required.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `AuthStore` | Frontend page | [`../../../../frontend/projects/wu-auth/src/lib/auth-store.ts`](../../../../frontend/projects/wu-auth/src/lib/auth-store.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `AuthApi` | Frontend API boundary | [`../../../../frontend/projects/wu-api/src/lib/services/auth-api.ts`](../../../../frontend/projects/wu-api/src/lib/services/auth-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `InvitationsController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/Authentication/InvitationsController.cs`](../../../../backend/src/WordUp.Api/Controllers/Authentication/InvitationsController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `AcceptInviteCommand` | Application slice | [`../../../../backend/src/WordUp.Application/Authentication/Invitations/AcceptInviteCommand.cs`](../../../../backend/src/WordUp.Application/Authentication/Invitations/AcceptInviteCommand.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `AcceptInviteCommandHandler` | Application slice | [`../../../../backend/src/WordUp.Application/Authentication/Invitations/AcceptInviteCommandHandler.cs`](../../../../backend/src/WordUp.Application/Authentication/Invitations/AcceptInviteCommandHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `AcceptInviteCommandValidator` | Application slice | [`../../../../backend/src/WordUp.Application/Authentication/Invitations/AcceptInviteCommandValidator.cs`](../../../../backend/src/WordUp.Application/Authentication/Invitations/AcceptInviteCommandValidator.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `Invitation` | Domain state | [`../../../../backend/src/WordUp.Domain/Authentication/Invitation.cs`](../../../../backend/src/WordUp.Domain/Authentication/Invitation.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `User` | Domain state | [`../../../../backend/src/WordUp.Domain/Authentication/User.cs`](../../../../backend/src/WordUp.Domain/Authentication/User.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `InvitationStatus` | Domain state | [`../../../../backend/src/WordUp.Domain/Authentication/InvitationStatus.cs`](../../../../backend/src/WordUp.Domain/Authentication/InvitationStatus.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `UserStatus` | Domain state | [`../../../../backend/src/WordUp.Domain/Authentication/UserStatus.cs`](../../../../backend/src/WordUp.Domain/Authentication/UserStatus.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `NotFoundException` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Common/NotFoundException.cs`](../../../../backend/src/WordUp.Application/Shared/Common/NotFoundException.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Manage pending invitations is an adjacent feature slice that owns other outcomes within the same primary L2 capability.

- `ConsentRequired.submit()` starts the browser interaction and maintains presentation state.

- `AuthApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `InvitationsController.PostAsync()` applies transport policy and constructs `AcceptInviteCommand`.

- `AcceptInviteCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `Invitation`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. A program administrator or account holder starts this feature interaction through the Portal application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-AUTH-004`, `L2-AUTH-008`, `L2-PLATFORM-011`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-AUTH-004` | `L1-AUTH-003` | An invitee shall accept by setting display name + password; staff shall be able to resend and revoke invitations; expired/revoked links shall not work. |
| `L2-AUTH-008` | `L1-AUTH-009` | All passwords shall satisfy a defined policy and be stored using a strong adaptive hash. |
| `L2-PLATFORM-011` | `L1-PLATFORM-003` | Youth-lifecycle gates shall present clear states and recovery. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Accept an invitation](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Accept an invitation. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Accept an invitation](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Accept an invitation. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Accept an invitation](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Accept an invitation. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Accept an invitation](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Accept an invitation. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Accept an invitation](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Accept an invitation. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Accept an invitation](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Accept an invitation. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
