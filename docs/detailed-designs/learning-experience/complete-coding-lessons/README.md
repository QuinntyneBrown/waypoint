# Complete a coding lesson

## Identification and scope

The Word Up software system is the entity of interest (EoI). The Complete a coding lesson feature slice (`FS-LEARN-COMPLETE-CODING-LESSONS`) is an architecture description element in the Learning Experience subsystem.

| Item | Identification |
| --- | --- |
| Participating applications | Portal application; WordUp API application |
| Architecture view | Application Context view (`V-APP-LEARN-COMPLETE-CODING-LESSONS`) |
| Governing viewpoint | Application Context viewpoint (`VP-APP-CONTEXT`) |
| View component | Nested Application Context component (`VC-APP-LEARN-COMPLETE-CODING-LESSONS`) |

This focused description addresses feature scope, behavior, application context, and requirement correspondences. It does not claim conformance as a complete architecture description.

## Overview

The Complete a coding lesson feature slice provides a youth an interactive, stepwise coding lesson with executable feedback and progress.

## Application context

The Application Context view (`V-APP-LEARN-COMPLETE-CODING-LESSONS`) applies the Application Context viewpoint (`VP-APP-CONTEXT`) to the Word Up software system. The viewpoint frames the application-location concern (`C-APP-LOCATION`). The view component (`VC-APP-LEARN-COMPLETE-CODING-LESSONS`) uses the nested-containment model kind.

Nested rectangles identify the EoI, each participating application, the subsystem boundary, and the feature-slice contribution. Figure 1 presents the view component.

![Figure 1 — Application Context view for the Complete a coding lesson feature slice.](diagrams/application-context.png)

*Figure 1 — Application Context view: the Complete a coding lesson feature slice within the participating Word Up applications.*

## Behavior

- **Entry.** A youth opens an unlocked coding lesson.
- **Processing.** The page combines instructional steps, code samples, an editor and preview, and a completion checklist; server-validated completion records progress toward relevant badge rules.
- **Result.** The lesson is complete and curriculum progression reflects the coding achievement.

## Scope boundary

General lesson authoring and project submission are separate slices.

## Correspondences

The requirement-trace correspondence method (`M-REQ-TRACE`) relates stable L2 requirement identifiers to feature slices. The Complete a coding lesson requirement correspondence (`COR-REQ-LEARN-COMPLETE-CODING-LESSONS`) records the following relationships.

| Requirement | Relationship |
| --- | --- |
| `L2-LEARN-008` | Primary requirement |
| `L2-LEARN-001` | Dependency |
| `L2-CURRIC-005` | Dependency |
