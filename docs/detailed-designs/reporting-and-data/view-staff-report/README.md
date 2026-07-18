# View a staff report

## Identification and scope

The Word Up software system is the entity of interest (EoI). The View a staff report feature slice (`FS-REPORT-VIEW-STAFF-REPORT`) is an architecture description element in the Reporting and Data subsystem.

| Item | Identification |
| --- | --- |
| Participating applications | Portal application; WordUp API application |
| Architecture view | Application Context view (`V-APP-REPORT-VIEW-STAFF-REPORT`) |
| Governing viewpoint | Application Context viewpoint (`VP-APP-CONTEXT`) |
| View component | Nested Application Context component (`VC-APP-REPORT-VIEW-STAFF-REPORT`) |

This focused description addresses feature scope, behavior, application context, and requirement correspondences. It does not claim conformance as a complete architecture description.

## Overview

The View a staff report feature slice helps administrators understand mentor activity and caseload balance.

## Application context

The Application Context view (`V-APP-REPORT-VIEW-STAFF-REPORT`) applies the Application Context viewpoint (`VP-APP-CONTEXT`) to the Word Up software system. The viewpoint frames the application-location concern (`C-APP-LOCATION`). The view component (`VC-APP-REPORT-VIEW-STAFF-REPORT`) uses the nested-containment model kind.

Nested rectangles identify the EoI, each participating application, the subsystem boundary, and the feature-slice contribution. Figure 1 presents the view component.

![Figure 1 — Application Context view for the View a staff report feature slice.](diagrams/application-context.png)

*Figure 1 — Application Context view: the View a staff report feature slice within the participating Word Up applications.*

## Behavior

- **Entry.** An authorized administrator opens the Staff Activity report with a reporting scope.
- **Processing.** The server computes per-mentor assigned youth, completed check-ins, reviews, target-band load balance, and overload warnings from authoritative assignments and activity.
- **Result.** Program leadership can spot imbalanced workloads and areas needing support.

## Scope boundary

Changing mentor assignments occurs in Program Administration.

## Correspondences

The requirement-trace correspondence method (`M-REQ-TRACE`) relates stable L2 requirement identifiers to feature slices. The View a staff report requirement correspondence (`COR-REQ-REPORT-VIEW-STAFF-REPORT`) records the following relationships.

| Requirement | Relationship |
| --- | --- |
| `L2-REPORT-001` | Primary requirement |
| `L2-ADMIN-005` | Dependency |
