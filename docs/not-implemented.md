# Repository implementation gaps

Snapshot: **2026-07-20**

The 2026-07-18 source audit that previously lived here has been closed. Its missing role routes,
public forms, learning and mentoring surfaces, cross-cutting backend work, and quality gates are now
implemented and recorded item-by-item in the [gap-closure plan](gap-closure-plan.md).

## Current status

There is no open, in-scope repository implementation item for the engineering-completion boundary
defined in [implementation-plan.md](implementation-plan.md). In particular:

- all youth, mentor, guardian, volunteer, and program-administrator homes and navigation targets
  resolve, with the route-manifest gate checking the production route graph;
- all 88 in-scope L2 requirement IDs have acceptance-test references and pass the strict
  requirement-level traceability gate;
- invitation lifecycle, configurable SMTP delivery, durable email and realtime post-commit work,
  scheduled notifications, help, and administrator/self-service privacy workflows are wired end to
  end;
- SQL-backed 50-user/20-RPS, throttled youth-screen, XS–XL responsive, WCAG 2.1 AA, build, unit,
  integration, and browser gates are part of the repository and CI; and
- the only excluded product ideas remain those explicitly listed in
  [specs/out-of-scope.md](specs/out-of-scope.md).

The traceability script also offers an optional per-acceptance-criterion citation audit. CI enforces
the strict 88-requirement gate; per-criterion citation density is supporting documentation evidence,
not a claim that every criterion has its own isolated test.

## Not a production-launch certification

Repository completion does not authorize real participant data or a production launch. Hosting,
secret provisioning, live SMTP verification, backup/restore and disaster-recovery exercises,
monitoring/on-call ownership, privacy-workflow data-volume and query-plan validation for indexed
ownership links, independent security/privacy/safeguarding/accessibility
reviews, and stakeholder acceptance require environment-specific evidence and accountable owners.
Those release activities are tracked in the production-release section of
[implementation-plan.md](implementation-plan.md), not as missing product implementation.

## Keeping this inventory accurate

If an in-scope behavior regresses or a requirement is added, add a concrete unchecked item to
[gap-closure-plan.md](gap-closure-plan.md) and describe its user-visible outcome and acceptance
evidence here. Do not treat static files under `docs/mocks/` as production implementation, and do not
reopen an explicitly excluded feature without first changing the scope authority under `docs/specs/`.
