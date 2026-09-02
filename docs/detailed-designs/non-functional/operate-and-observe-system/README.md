# Operate and observe the system

## Overview

Waypoint is a youth-leadership software system for program intake, learning, mentoring, safeguarding, and program operations. The Non-Functional and Quality subsystem owns this feature slice.

The Operate and observe the system feature slice refines `L2-NFR-011` into one independently deliverable interaction. The system shall expose health checks, structured logging, and an admin CLI (operational tooling — an out-of-band ops interface, not product scope).

A platform operator starts this feature interaction through the out-of-band `way` command-line tool. The tool composes the same application and infrastructure layers as the API, attributes state changes to an active Program Administrator, and presents structured command results on standard output.

The Waypoint software system is the entity of interest (EoI). This focused architecture description (AD) addresses the feature boundary, participating applications, software structure, requirements, and behavior. It does not claim conformance as a complete AD; stakeholder perspectives, complete concern coverage, correspondences outside requirement traceability, and architecture decisions remain in the system-level AD.

## Description

### Vertical slice boundary

The feature spans the Waypoint API, CLI, application, and infrastructure projects. The API owns health and request telemetry; the CLI owns operational parsing and presentation. Both use the same application handlers, validation pipeline, domain rules, audit path, and persisted state. Adjacent subsystem feature slices own behavior outside this end-to-end outcome.

### Components and types

A source link identifies an existing realization. An unlinked name identifies a planned feature boundary where the repository has no dedicated type.

| Layer | Components, types, and classes | Responsibility |
| --- | --- | --- |
| Operational transport | [`CliApplication`](../../../../backend/src/Waypoint.Cli/CliApplication.cs)<br>[`Program`](../../../../backend/src/Waypoint.Cli/Program.cs) | Defines typed `way` commands, composes the generic host, selects an operator, and maps safe results/errors to stdout/stderr. |
| API transport | [`Program`](../../../../backend/src/Waypoint.Api/Program.cs)<br>[`RequestLoggingMiddleware`](../../../../backend/src/Waypoint.Api/Observability/RequestLoggingMiddleware.cs) | Exposes health endpoints, validates production configuration, applies migrations before serving, and emits redacted structured telemetry. |
| Application slice | [`CreateOperationalUserCommand`](../../../../backend/src/Waypoint.Application/OperationalAdministration/CreateOperationalUserCommand.cs)<br>[`ListInvitationsQuery`](../../../../backend/src/Waypoint.Application/OperationalAdministration/ListInvitationsQuery.cs)<br>[`IssueOperationalTokenQuery`](../../../../backend/src/Waypoint.Application/OperationalAdministration/IssueOperationalTokenQuery.cs) | Adds operational use cases where no API command already exists; all other CLI leaves send the existing application command/query through MediatR. |
| Infrastructure and shared ports | [`DatabaseLifecycle`](../../../../backend/src/Waypoint.Infrastructure/DatabaseLifecycle.cs)<br>[`InfrastructureStartupValidator`](../../../../backend/src/Waypoint.Infrastructure/InfrastructureStartupValidator.cs)<br>[`AppDbContext`](../../../../backend/src/Waypoint.Infrastructure/AppDbContext.cs) | Supplies durable migration/reset behavior, fail-fast production configuration validation, persistence, audit, email, hashing, and token adapters. |

### Functions and data flow

- `Program.Main()` composes `AddApplication()` and `AddInfrastructure()` through a generic host after production configuration validation.
- `CliApplication.RunAsync()` parses typed arguments/options and invokes the selected leaf command.
- The CLI selects an active Program Administrator (or the explicit `--actor`) before sending application requests through `ISender`.
- Existing and operational application handlers load authorized state through `IAppDbContext`, apply validation and domain rules, and return typed results.
- `IAppDbContext.SaveChangesAsync()` commits one unit of work when the feature changes state. Read-only flows use bounded, no-tracking projections.
- `DatabaseLifecycle.MigrateAsync()` applies pending EF Core migrations; API startup completes this step before accepting requests.

### Behavior

1. **Entry.** A platform operator runs a typed `way` command. Help and version output require no production configuration.
2. **Processing.** Destructive reset requires `--confirm RESET`; other state-changing commands resolve an active Program Administrator for audit attribution and send the corresponding request through the shared validation/application pipeline.
3. **Result.** The handler commits an audited state change when applicable. The CLI returns JSON, a generated report path, or a concise status line and a deterministic exit code. Production API startup separately validates durable configuration and applies pending migrations before serving.

## Requirements

This section contains L2 specification text and L1 trace identifiers. It contains no L1 specification text. Normative wording uses `shall` in accordance with the architecture-description voice.

### L2-NFR-011 — Observability and operations

| Field | Value |
| --- | --- |
| Relationship | Primary |
| L1 specification ID | `L1-NFR-008` |

The system shall expose health checks, structured logging, and an admin CLI (operational tooling — an out-of-band ops interface, not product scope).

## Diagrams

The six figures use one visual vocabulary. Blue boxes identify frontend elements, green boxes identify backend application elements, purple boxes identify domain or data elements, and orange boxes identify infrastructure or external elements. Nested boxes identify the software boundary, the frontend or backend boundary, and the owning application.

### C4 diagrams

#### System context

Figure 1 is a view component governed by the C4 system-context model kind. It identifies the people and external software systems that interact with the Waypoint software system for this feature.

![Figure 1 — C4 system context for Operate and observe the system.](diagrams/c4-system-context.png)

*Figure 1 — C4 system context: Operate and observe the system in the Waypoint environment.*

#### Container

Figure 2 is a view component governed by the C4 container model kind. It separates frontend containers from backend and persistence containers.

![Figure 2 — C4 container view for Operate and observe the system.](diagrams/c4-container.png)

*Figure 2 — C4 container view: application and data boundaries for Operate and observe the system.*

#### Component

Figure 3 is a view component governed by the C4 component model kind. It locates the page, typed client, controller, handler, domain type, and persistence adapter inside their owning applications.

![Figure 3 — C4 component view for Operate and observe the system.](diagrams/c4-component.png)

*Figure 3 — C4 component view: collaborating components for Operate and observe the system.*

#### Code

Figure 4 is a view component governed by the C4 code model kind. It shows the principal code elements and their dependency direction.

![Figure 4 — C4 code view for Operate and observe the system.](diagrams/c4-code.png)

*Figure 4 — C4 code view: feature-owned code elements for Operate and observe the system.*

### Class diagram

Figure 5 is a view component governed by the UML class-structure model kind. Solid relationships show runtime collaboration, dashed relationships show construction, and aggregation shows persistence ownership.

![Figure 5 — Class structure for Operate and observe the system.](diagrams/class-structure.png)

*Figure 5 — Class structure: relationships among frontend, API, application, domain, and persistence types for Operate and observe the system.*

### Sequence diagram

Figure 6 is a view component governed by the UML sequence model kind. Frontend and backend boxes identify the application context for each participant.

![Figure 6 — Sequence behavior for Operate and observe the system.](diagrams/sequence-behaviour.png)

*Figure 6 — Sequence behavior: the end-to-end operate and observe the system interaction.*
