# Waypoint — Requirements Specification

Requirements are organized **by subsystem**. Each subsystem has its own folder containing an `L1.md` (high-level requirements) and `L2.md` (detailed requirements with Given/When/Then acceptance criteria).

## Requirement ID scheme

`L{level}-{SUBSYSTEM}-{NNN}` — e.g. `L1-AUTH-001`, `L2-AUTH-012`.

- `level` = `1` (high-level) or `2` (detailed).
- `SUBSYSTEM` = the subsystem code (see table below).
- `NNN` = zero-padded sequence number that **restarts at 001 within each subsystem**.

Every L2 traces to an L1 **in the same subsystem**. Cross-subsystem dependencies are expressed as explicit `(see L2-XXX-NNN)` references inside acceptance criteria. IDs are **stable**: when a requirement is descoped it is removed and its ID **retired (never reused)**, so existing cross-references stay valid — descoped items are recorded in `out-of-scope.md`.

## Subsystems

| Code | Subsystem | Folder | L1 | L2 |
|---|---|---|---|---|
| PUB | Public Site | `public-site/` | 1 | 3 |
| INTAKE | Application Intake | `application-intake/` | 2 | 2 |
| AUTH | Authentication & Access | `authentication/` | 8 | 11 |
| CONSENT | Consent & Safeguarding | `consent-and-safeguarding/` | 3 | 4 |
| ADMIN | Program Administration | `program-administration/` | 4 | 6 |
| CURRIC | Curriculum Authoring | `curriculum-authoring/` | 5 | 6 |
| LEARN | Learning Experience | `learning-experience/` | 8 | 10 |
| MENTOR | Mentoring & Coaching | `mentoring/` | 8 | 8 |
| ENGAGE | Engagement & Operations | `engagement/` | 6 | 8 |
| GUARDIAN | Guardian Portal | `guardian-portal/` | 1 | 3 |
| VOLUNTEER | Volunteer Portal | `volunteer-portal/` | 1 | 2 |
| REPORT | Reporting & Data | `reporting-and-data/` | 3 | 3 |
| PLATFORM | Platform Services | `platform-services/` | 2 | 11 |
| NFR | Non-Functional & Quality | `non-functional/` | 9 | 11 |
| **Total** | **14 subsystems** | | **61** | **88** |

Features intentionally excluded from v1 are recorded in **`out-of-scope.md`**, with the PRD rationale and the IDs they were removed from.

## ATDD convention

Every acceptance test must carry a trace header identifying the L2(s) it covers, e.g. `// Traces to: L2-AUTH-001` (TypeScript) or `# Traces to: L2-AUTH-001` (Python). The trace comment is mandatory — it is how requirement coverage is verified.

## Source

Requirements are derived from the **Waypoint PRD** (`docs/PRD.md`), the single source of truth. Personas: Youth, Mentor (**Lead Mentor** is a per-cohort designation, not a system role), Program Administrator, Guardian, Volunteer, plus unauthenticated Public visitors.

---

## Appendix A — Product Decisions to Confirm

Each item below is the **canonical rule now stated in the owning subsystem's L2**; the product owner should confirm the chosen value before implementation.

- **A-1 · Cohort status.** Cohort status has a single authoritative source (see `public-site/L2.md` L2-PUB-002).
- **A-2 · Youth eligibility age.** Ages **12–18 inclusive** (see `application-intake/L2.md` L2-INTAKE-001).
- **A-3 · Invitation expiry.** **14 days** after issuance (see `authentication/L2.md` L2-AUTH-003).
- **A-4 · Password policy.** At least **8 characters including at least one letter and one number** (see `authentication/L2.md` L2-AUTH-008).
- **A-5 · Roles.** Canonical system roles: **Youth, Mentor, Program Administrator, Guardian, Volunteer, Public**; **Lead Mentor** is a per-cohort designation, not a system role (see `authentication/L2.md` L2-AUTH-009).
- **A-6 · Quiz attempts & scoring.** Recorded score is the **best passing attempt within the configured attempt limit** (see `learning-experience/L2.md` L2-LEARN-004; authoring setting in `curriculum-authoring/L2.md` L2-CURRIC-003).
- **A-7 · Progress/badge/streak counts.** Computed from a **single source of truth** (see `learning-experience/L2.md` L2-LEARN-009).
- **A-8 · Federated sign-in.** Google/federated sign-in is **out of scope for v1** (see `out-of-scope.md`); v1 uses email/password invite-based authentication.
- **A-9 · Donations.** The donation pathway uses an **external PCI-compliant provider** (no card data stored); the recurring/monthly-tier model is a product decision to confirm (see `public-site/L2.md` L2-PUB-003).
- **A-10 · Theme.** No light/dark theme toggle in v1; display preferences cover text size, contrast, motion, and reading font.

## Appendix B — Cross-subsystem dependency notes

- **Authorization** (`AUTH`) is enforced by every subsystem's server endpoints; functional L2s reference `L2-AUTH-009`/`L2-AUTH-010`.
- **Audit logging** (`CONSENT` L2-CONSENT-003) is written by mutating actions across ADMIN, MENTOR, CURRIC, INTAKE, ENGAGE.
- **Offline attendance** (`NFR` L2-NFR-004) underpins ENGAGE and VOLUNTEER attendance capture. Offline is limited to attendance (see `out-of-scope.md`).
- **Near-real-time delivery** (`NFR` L2-NFR-005) delivers **notifications and messages** to LEARN, MENTOR, ENGAGE, and ADMIN clients. Broader live domain-event fan-out is out of scope (see `out-of-scope.md`).
- **Data export** (`REPORT` L2-REPORT-003) — **CSV/PDF** — is invoked by the ADMIN progress board and ENGAGE attendance history.
- **Responsive** (`NFR` L2-NFR-009) and **accessibility** (`NFR` L2-NFR-010) apply to every UI-facing subsystem.
