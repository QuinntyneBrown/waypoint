# Recover a password — Detailed design

## Overview

Recover a password — vertical slice that refines `L2-AUTH-005` into one independently deliverable interaction. Users shall be able to request a reset link and set a new password without the system disclosing account existence.

Word Up is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Authentication and Access subsystem owns this feature slice.

An account holder starts this feature interaction through the Portal application. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

The entity of interest (EoI) is the Word Up software system. This focused architecture description (AD) describes the Recover a password vertical slice and does not claim full conformance with 42010:2022.

## Description

### Components, types, functions, and classes

| Element | Kind | Source | Responsibility and public interface |
| --- | --- | --- | --- |
| `ResetPassword` | Frontend page | [`../../../../frontend/projects/portal/src/app/auth/reset-password/reset-password.ts`](../../../../frontend/projects/portal/src/app/auth/reset-password/reset-password.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `ForgotPassword` | Frontend page | [`../../../../frontend/projects/portal/src/app/auth/forgot-password/forgot-password.ts`](../../../../frontend/projects/portal/src/app/auth/forgot-password/forgot-password.ts) | Collects feature input, invokes the typed client, and renders loading, success, empty, forbidden, and error states. |
| `PasswordResetApi` | Frontend API boundary | [`../../../../frontend/projects/wu-api/src/lib/services/password-reset-api.ts`](../../../../frontend/projects/wu-api/src/lib/services/password-reset-api.ts) | Serializes requests, deserializes typed results, and maps safe API errors for the page. |
| `PasswordResetController` | API transport | [`../../../../backend/src/WordUp.Api/Controllers/Authentication/PasswordResetController.cs`](../../../../backend/src/WordUp.Api/Controllers/Authentication/PasswordResetController.cs) | Applies endpoint authentication and policy, maps request contracts, and sends the application request through MediatR. |
| `ResetPasswordCommand` | Application slice | [`../../../../backend/src/WordUp.Application/Authentication/PasswordReset/ResetPasswordCommand.cs`](../../../../backend/src/WordUp.Application/Authentication/PasswordReset/ResetPasswordCommand.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `ResetPasswordCommandHandler` | Application slice | [`../../../../backend/src/WordUp.Application/Authentication/PasswordReset/ResetPasswordCommandHandler.cs`](../../../../backend/src/WordUp.Application/Authentication/PasswordReset/ResetPasswordCommandHandler.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `ResetPasswordCommandValidator` | Application slice | [`../../../../backend/src/WordUp.Application/Authentication/PasswordReset/ResetPasswordCommandValidator.cs`](../../../../backend/src/WordUp.Application/Authentication/PasswordReset/ResetPasswordCommandValidator.cs) | Coordinates the use case, performs resource-level scoping, invokes domain behavior, and returns the result. |
| `PasswordResetToken` | Domain state | [`../../../../backend/src/WordUp.Domain/Authentication/PasswordResetToken.cs`](../../../../backend/src/WordUp.Domain/Authentication/PasswordResetToken.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `User` | Domain state | [`../../../../backend/src/WordUp.Domain/Authentication/User.cs`](../../../../backend/src/WordUp.Domain/Authentication/User.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `RefreshToken` | Domain state | [`../../../../backend/src/WordUp.Domain/Authentication/RefreshToken.cs`](../../../../backend/src/WordUp.Domain/Authentication/RefreshToken.cs) | Holds persistent state and lifecycle invariants used by the feature. |
| `IJwtTokenIssuer` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Abstractions/IJwtTokenIssuer.cs`](../../../../backend/src/WordUp.Application/Shared/Abstractions/IJwtTokenIssuer.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |
| `ISecureTokens` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Application/Shared/Abstractions/ISecureTokens.cs`](../../../../backend/src/WordUp.Application/Shared/Abstractions/ISecureTokens.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |
| `SecurityHeadersMiddleware` | Infrastructure and shared ports | [`../../../../backend/src/WordUp.Api/Middleware/SecurityHeadersMiddleware.cs`](../../../../backend/src/WordUp.Api/Middleware/SecurityHeadersMiddleware.cs) | Supplies persistence, current-user context, time, audit, email, or realtime adapters when the feature requires them. |

### Structure and relationships

- The feature spans Portal application; WordUp API application. The frontend application owns interaction state and presentation. The WordUp API application remains authoritative for authentication, authorization, validation, domain rules, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

- `ResetPassword.submit()` starts the browser interaction and maintains presentation state.

- `PasswordResetApi.execute()` crosses the frontend-to-backend boundary with a typed HTTP request.

- `PasswordResetController.PostAsync()` applies transport policy and constructs `ResetPasswordCommand`.

- `ResetPasswordCommandHandler.Handle()` loads authorized state through `IAppDbContext`, applies feature rules through `PasswordResetToken`, and returns a typed result.

- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.

### Behaviour

1. An account holder starts this feature interaction through the Portal application.

2. The frontend application captures the interaction and sends a typed request to the WordUp API application. The controller applies endpoint policy. The application handler applies `L2-AUTH-005`, `L2-AUTH-008`, `L2-AUTH-012`, scopes the operation to the current actor, and queries or changes domain state through the persistence port.

3. The backend commits an authorized state change when applicable, and the frontend application presents the authoritative feature result.

## Requirements

This section contains L2 requirements only. It intentionally includes no L1 requirement text. The L1 specification identifier records the traceability correspondence for each L2 requirement.

| L2 specification ID | L1 specification ID | Requirement text |
| --- | --- | --- |
| `L2-AUTH-005` | `L1-AUTH-004` | Users shall be able to request a reset link and set a new password without the system disclosing account existence. |
| `L2-AUTH-008` | `L1-AUTH-009` | All passwords shall satisfy a defined policy and be stored using a strong adaptive hash. |
| `L2-AUTH-012` | `L1-AUTH-009` | Access/refresh tokens shall be issued, stored, and rotated securely. |

## Diagrams

The six architecture views use one caption pattern and stable EoI-local names. Each view component is available as PlantUML source and as an inline Portable Network Graphics (PNG) rendering.

### C4 system context view

[PlantUML source](diagrams/c4-system-context.puml)

![Figure 1 — C4 system context view for Recover a password](diagrams/c4-system-context.png)

Figure 1 — C4 system context view: the Word Up software system, its actors, and its external dependencies for Recover a password. The view component uses the C4 system context model kind.

### C4 container view

[PlantUML source](diagrams/c4-container.puml)

![Figure 2 — C4 container view for Recover a password](diagrams/c4-container.png)

Figure 2 — C4 container view: the frontend, backend, data, and integration boundaries for Recover a password. The view component uses the C4 container model kind.

### C4 component view

[PlantUML source](diagrams/c4-component.puml)

![Figure 3 — C4 component view for Recover a password](diagrams/c4-component.png)

Figure 3 — C4 component view: the source-level components and their structural relationships for Recover a password. The view component uses the C4 component model kind.

### C4 code view

[PlantUML source](diagrams/c4-code.puml)

![Figure 4 — C4 code view for Recover a password](diagrams/c4-code.png)

Figure 4 — C4 code view: the principal code elements and their dependency direction for Recover a password. The view component uses the C4 code model kind.

### Class view

[PlantUML source](diagrams/class-structure.puml)

![Figure 5 — Class view for Recover a password](diagrams/class-structure.png)

Figure 5 — Class view: the feature types, functions, classes, entities, and their relationships for Recover a password. The view component uses the Unified Modeling Language (UML) class model kind.

### Sequence view

[PlantUML source](diagrams/sequence-behaviour.puml)

![Figure 6 — Sequence view for Recover a password](diagrams/sequence-behaviour.png)

Figure 6 — Sequence view: the principal end-to-end feature behaviour for Recover a password. Nested application boxes separate frontend behaviour from backend behaviour. The view component uses the UML sequence model kind.
