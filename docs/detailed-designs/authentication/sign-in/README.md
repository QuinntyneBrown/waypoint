# Sign in

## Overview

Waypoint is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Authentication and Access subsystem owns this feature slice.

The Sign in feature slice refines `L2-AUTH-001` into one independently deliverable interaction. The system shall authenticate by email + password and issue an access token plus a rotating refresh token on success.

An account holder starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The Waypoint software system is the entity of interest (EoI). This focused architecture description (AD) addresses the feature boundary, participating applications, software structure, requirements, and behavior. It does not claim conformance as a complete AD; stakeholder perspectives, complete concern coverage, correspondences outside requirement traceability, and architecture decisions remain in the system-level AD.

## Description

### Vertical slice boundary

The feature spans Portal application; Waypoint API application. The frontend application owns interaction state and presentation. The Waypoint API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

### Components and types

A source link identifies an existing realization. An unlinked name identifies a planned feature boundary where the repository has no dedicated type.

| Layer | Components, types, and classes | Responsibility |
| --- | --- | --- |
| Frontend page | [`SignIn`](../../../../frontend/projects/auth/pages/src/sign-in/sign-in.ts)<br>[`AuthStore`](../../../../frontend/projects/auth/src/lib/auth-store.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| Frontend API boundary | [`AuthService`](../../../../frontend/projects/api/src/lib/services/auth.service.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| API transport | [`AuthController`](../../../../backend/src/Waypoint.Api/Controllers/Authentication/AuthController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| Application slice | [`SignInCommand`](../../../../backend/src/Waypoint.Application/Authentication/Auth/SignInCommand.cs)<br>[`SignInCommandHandler`](../../../../backend/src/Waypoint.Application/Authentication/Auth/SignInCommandHandler.cs)<br>[`SignInCommandValidator`](../../../../backend/src/Waypoint.Application/Authentication/Auth/SignInCommandValidator.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| Domain state | [`User`](../../../../backend/src/Waypoint.Domain/Authentication/User.cs)<br>[`UserStatus`](../../../../backend/src/Waypoint.Domain/Authentication/UserStatus.cs)<br>[`PasswordResetToken`](../../../../backend/src/Waypoint.Domain/Authentication/PasswordResetToken.cs)<br>[`RefreshToken`](../../../../backend/src/Waypoint.Domain/Authentication/RefreshToken.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| Infrastructure and shared ports | [`SecurityHeadersMiddleware`](../../../../backend/src/Waypoint.Api/Middleware/SecurityHeadersMiddleware.cs)<br>[`IJwtTokenIssuer`](../../../../backend/src/Waypoint.Application/Shared/Abstractions/IJwtTokenIssuer.cs)<br>[`ISecureTokens`](../../../../backend/src/Waypoint.Application/Shared/Abstractions/ISecureTokens.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Functions and data flow

- `SignIn.submit()` starts the browser interaction and maintains presentation state.
- `AuthService.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.
- `AuthController.PostAsync()` applies transport policy and constructs `SignInCommand`.
- `SignInCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `User`, and returns a typed result.
- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behavior

1. **Entry.** An account holder starts this feature interaction through the Portal application.
2. **Processing.** The frontend application captures the interaction and sends a typed request to the Waypoint API application. The controller applies endpoint policy. The application handler applies `L2-AUTH-001`, `L2-AUTH-006`, `L2-AUTH-012`, `L2-NFR-002`, `L2-CONSENT-003`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.
3. **Result.** The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 specification text and L1 trace identifiers. It contains no L1 specification text. Normative wording uses `shall` in accordance with the architecture-description voice.

### L2-AUTH-001 — Email/password sign-in

| Field | Value |
| --- | --- |
| Relationship | Primary |
| L1 specification ID | `L1-AUTH-001` |

The system shall authenticate by email + password and issue an access token plus a rotating refresh token on success.

### L2-AUTH-006 — Account lockout

| Field | Value |
| --- | --- |
| Relationship | Dependency |
| L1 specification ID | `L1-AUTH-005` |

After too many failed attempts, the account shall lock for a defined duration and offer recovery.

### L2-AUTH-012 — Token security and session management

| Field | Value |
| --- | --- |
| Relationship | Dependency |
| L1 specification ID | `L1-AUTH-009` |

Access/refresh tokens shall be issued, stored, and rotated securely.

### L2-NFR-002 — CSRF, CORS, security headers, and rate limiting

| Field | Value |
| --- | --- |
| Relationship | Dependency |
| L1 specification ID | `L1-NFR-001` |

The API shall protect against cross-site attacks, set appropriate headers, and rate-limit sensitive endpoints.

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

![Figure 1 — C4 system context for Sign in.](diagrams/c4-system-context.png)

*Figure 1 — C4 system context: Sign in in the Waypoint environment.*

#### Container

Figure 2 is a view component governed by the C4 container model kind. It separates frontend containers from backend and persistence containers.

![Figure 2 — C4 container view for Sign in.](diagrams/c4-container.png)

*Figure 2 — C4 container view: application and data boundaries for Sign in.*

#### Component

Figure 3 is a view component governed by the C4 component model kind. It locates the page, typed client, controller, handler, domain type, and persistence adapter inside their owning applications.

![Figure 3 — C4 component view for Sign in.](diagrams/c4-component.png)

*Figure 3 — C4 component view: collaborating components for Sign in.*

#### Code

Figure 4 is a view component governed by the C4 code model kind. It shows the principal code elements and their dependency direction.

![Figure 4 — C4 code view for Sign in.](diagrams/c4-code.png)

*Figure 4 — C4 code view: feature-owned code elements for Sign in.*

### Class diagram

Figure 5 is a view component governed by the UML class-structure model kind. Solid relationships show runtime collaboration, dashed relationships show construction, and aggregation shows persistence ownership.

![Figure 5 — Class structure for Sign in.](diagrams/class-structure.png)

*Figure 5 — Class structure: relationships among frontend, API, application, domain, and persistence types for Sign in.*

### Sequence diagram

Figure 6 is a view component governed by the UML sequence model kind. Frontend and backend boxes identify the application context for each participant.

![Figure 6 — Sequence behavior for Sign in.](diagrams/sequence-behaviour.png)

*Figure 6 — Sequence behavior: the end-to-end sign in interaction.*
