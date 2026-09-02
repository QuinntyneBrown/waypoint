# Waypoint — Backend Implementation Plan

> **Status (2026-07-20): completed and retained as a historical delivery record.** The
> repository's engineering-completion boundary and current verification gates are summarized in
> [implementation-plan.md](implementation-plan.md). The implementation now targets .NET 9; stack,
> scope, and "NEW" path statements below describe the baseline plan as it was originally written.
> This status is not a production-launch certification.

> Historical target: a production-grade API that powers the Waypoint prototype
> (`docs/mocks/index.html`). Planned stack: **.NET 8 · Clean Architecture · Controllers · MediatR
> (free) · EF Core + SQL Server/SQL Express · JWT OAuth2 · SignalR · NUnit · System.CommandLine
> dotnet tool.**

---

## 1. Context

The front-end mockups in this repo define a youth-leadership tracking app for **Waypoint** (director **Shawn Peters**; personas Youth, Mentor, Director, Guardian, Volunteer). This document plans the **backend** that turns those mockups into a real system: authenticate users, store the curriculum + progression data currently held in Google Sheets, track every youth's journey, push real-time updates, and expose an admin CLI for operations and the Sheets migration.

The design **matches the developer's existing house style** (the `Forge` clean-architecture template and the `mvp`/`CodeGenerator` dotnet tools) so it drops into the same ecosystem, with the deltas the request calls out (NUnit instead of xUnit; SignalR + an admin CLI added).

### Where it lives

Monorepo alongside the mockups:

```
waypoint/
  docs/
    mocks/                                   # existing front-end prototype (index.html + subsystem folders)
    contract.md                              # front-end build contract
    backend-implementation-plan.md           # this file
    specs/                                   # L1/L2 requirements (acceptance criteria) — every acceptance test traces to an L2 id
    icd/                                     # Interface Control Document (§12)
  backend/                                   # NEW — this plan
    Directory.Build.props
    Waypoint.sln
    src/{Waypoint.Domain, Waypoint.Application, Waypoint.Infrastructure, Waypoint.Api, Waypoint.Cli}
    tests/{Waypoint.UnitTests, Waypoint.IntegrationTests}
```

---

## 2. Key decisions (and why)

| Area           | Decision                                                                                                                            | Rationale                                                                                                                                                                                                                                                                     |
| -------------- | ----------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Framework      | **.NET 8** (`net8.0`)                                                                                                               | As requested. House `Directory.Build.props` pattern, retargeted from Forge's net9.0.                                                                                                                                                                                          |
| Architecture   | **Clean Architecture**, 4 projects (Domain/Application/Infrastructure/Api) + CLI + tests                                            | Matches Forge exactly; dependencies flow inward.                                                                                                                                                                                                                              |
| API surface    | **MVC Controllers** (not Minimal APIs), thin, delegate to MediatR                                                                   | House convention; requested "Controllers".                                                                                                                                                                                                                                    |
| Mediator       | **MediatR `12.5.0`**                                                                                                                | Last **Apache-2.0 / free** release before v13 went dual-license (RPL 1.5 + commercial) on 2025-07-02.¹ House pins 12.x. _(Aside: v13 has a free Community edition for nonprofits/<$5M revenue — Waypoint qualifies — but 12.5.0 avoids the question and matches house style.)_ |
| Validation     | **FluentValidation `11.10.0`** via a `ValidationBehavior` pipeline                                                                  | House convention.                                                                                                                                                                                                                                                             |
| Error model    | **Typed exceptions → `ProblemDetails` middleware** (RFC 7807)                                                                       | Forge's canonical style. _(Alternative: `Result<T>` + `ToActionResult` as in `appbuilder` — pick one, don't mix. See §16.)_                                                                                                                                                   |
| Persistence    | **EF Core `8.x` + SQL Server / SQL Express**                                                                                        | Requested. `Server=.\SQLEXPRESS` for local, full SQL Server in prod. Migrations live in Infrastructure.                                                                                                                                                                       |
| AuthN/AuthZ    | **JWT bearer (OAuth2 password + refresh)** via an `IJwtTokenIssuer` port; role + policy authorization                               | Forge's hand-rolled JWT approach; roles map to the 5 personas.                                                                                                                                                                                                                |
| Real-time      | **SignalR** via an Application-side `IChangePublisher` port + Api-side `IHubContext` adapter                                        | Keeps handlers transport-agnostic (the `appbuilder` pattern).                                                                                                                                                                                                                 |
| Testing        | **NUnit `4.x`** — unit + integration                                                                                                | As requested. _(House uses xUnit; we keep the house **patterns** — `WebApplicationFactory` + real SQL DB per fixture — and translate them to NUnit.)_                                                                                                                         |
| Admin CLI      | **`System.CommandLine 2.0.0-beta4`**, packaged as a **dotnet tool** (`way`)                                                          | Matches the `mvp`/`CodeGenerator.Cli` tools (same beta, `Host` DI, `PackAsTool`). GA 2.0.x exists but changed the API and dropped the Hosting package — see §10.²                                                                                                             |
| Interface spec | **ICD** in `docs/icd/` + live **OpenAPI/Swagger**                                                                                   | Requested. OpenAPI is generated; the ICD is the human contract (§12).                                                                                                                                                                                                         |
| DTO mapping    | **Manual LINQ projection** (no AutoMapper in app code)                                                                              | House convention.                                                                                                                                                                                                                                                             |
| Conventions    | **File-per-type**, **folder-per-feature** (vertical slice), file-scoped namespaces, records for messages/DTOs, classes for handlers | House convention.                                                                                                                                                                                                                                                             |

¹ MediatR licensing: [Jimmy Bogard — Licensing Update](https://www.jimmybogard.com/automapper-and-mediatr-licensing-update/) · [Commercial launch](https://github.com/LuckyPennySoftware/MediatR/discussions/1123) · [Milan Jovanović summary](https://milanjovanovic.tech/blog/mediatr-and-masstransit-going-commercial-what-this-means-for-you). 12.5.0 = last plain Apache-2.0.
² System.CommandLine: [GA path / beta5+](https://github.com/dotnet/command-line-api/issues/2576) · [migration guide](https://learn.microsoft.com/en-us/dotnet/standard/commandline/migration-guide-2.0.0-beta5).

---

## 3. Solution structure

Dependency direction: `Api → Application + Infrastructure`, `Infrastructure → Application`, `Application → Domain`, `Domain → (nothing)`. `Cli → Application + Infrastructure`.

```
backend/
  Directory.Build.props        # net8.0; Nullable=enable; ImplicitUsings=enable;
                               # TreatWarningsAsErrors=true; WarningsAsErrors=nullable
  Waypoint.sln
  src/
    Waypoint.Domain/             # entities + enums only. ZERO package refs.
    Waypoint.Application/        # CQRS handlers, DTOs, Abstractions (ports), Behaviors
    Waypoint.Infrastructure/     # AppDbContext, Migrations/, port adapters
    Waypoint.Api/                # Controllers, Contracts (request DTOs), Middleware,
                               # Realtime/ (hubs+adapter), Program.cs
    Waypoint.Cli/           # System.CommandLine dotnet tool (PackAsTool)
  tests/
    Waypoint.UnitTests/          # NUnit: handlers, validators, domain rules (fast, no DB)
    Waypoint.IntegrationTests/   # NUnit + WebApplicationFactory + real SQL DB per fixture
```

**File-per-type + folder-per-feature.** Every class/interface/enum/record is its own file. Application features are vertical slices:

```
Waypoint.Application/
  Abstractions/                # ports: IAppDbContext, IClock, ICurrentUser,
                               #        IJwtTokenIssuer, IPasswordHasher,
                               #        IRefreshTokenStore, IChangePublisher,
                               #        ISheetImporter, IReportRenderer
  Behaviors/ValidationBehavior.cs
  Common/ (PagedResult<T>, etc.)
  Auth/            SignInCommand.cs SignInCommandHandler.cs SignInCommandValidator.cs
                   RegisterCommand.cs RefreshTokenCommand.cs AcceptInviteCommand.cs
                   AuthResult.cs InvalidCredentialsException.cs …
  Cohorts/         CreateCohortCommand(+Handler+Validator).cs GetCohortByIdQuery(+Handler).cs
                   ListCohortsQuery(+Handler).cs CohortDto.cs CohortProgressQuery(+Handler).cs …
  Curriculum/      Tracks / Modules / Lessons / Quizzes / Assignments features
  Youth/           profiles, progress, journey
  Progress/        lesson completion, quiz attempts, submissions, reflections
  Competencies/    assessment + progression framework
  Attendance/      sessions + records
  CheckIns/  Flags/  Badges/  Announcements/  Messages/  Notifications/
  People/    Invitations/  Applications/  Reports/  Audit/  Consent/  Import/
```

`Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors>nullable</WarningsAsErrors>
  </PropertyGroup>
</Project>
```

---

## 4. Domain model

Entities are plain POCOs (`{ get; set; }`, `Guid Id`, no attributes, no base class) in `Waypoint.Domain`. Enums are **numbered from 1**, persisted as `int` (`HasConversion<int>()`), serialized as **strings** on the wire (`JsonStringEnumConverter`). Grouped by aggregate area (mapped from the mockups):

**Identity & access**

- `User` (Id, Email, DisplayName, PasswordHash, `UserRole` Role, `UserStatus` Status, CreatedAt, AvatarSeed)
- `RefreshToken` (UserId, TokenHash, FamilyId, ExpiresAt, RevokedAt) — rotation + reuse detection
- `Invitation` (Email, Role, CohortId?, MentorId?, TokenHash, `InviteStatus`, ExpiresAt)
- `Application` (intake) (ApplicantName, GuardianName, Neighbourhood, ReferredBy, `ApplicationStatus`) → approve ⇒ creates `User(Youth)` + `YouthProfile`
- `GuardianConsent` (YouthId, GuardianUserId, ParticipationConsent, MediaConsent, Signature, SignedAt)

**People & cohorts**

- `YouthProfile` (UserId, CohortId, MentorId, Age, GuardianUserId?, `YouthStatus` {New=1, OnTrack=2, AtRisk=3}, PersonalGoal, LastActiveAt)
- `Cohort` (Name, TermStart, TermEnd, MentorId, `CohortStatus`)
- Mentor/Guardian/Volunteer are `User`s with the matching `UserRole`; mentor↔youth is `YouthProfile.MentorId`.

**Curriculum** (`enum Track.Key { Bible=1, Coding=2 }`)

- `Track` (Name, Key, `PublishStatus`) → `Module` (TrackId, Name, Order, Description, Prerequisites, `PublishStatus`) → `Lesson` (ModuleId, Title, Order, ScriptureRef, TeachingContent, MediaUrl?, ReflectionPrompts[], LeaderGuide, `PublishStatus`)
- `Quiz` (ModuleId/LessonId, PassMark, AttemptsAllowed) → `QuizQuestion` (Text, `QuestionType`, Options[], CorrectOptionId)
- `Assignment` (LessonId/ModuleId, Title, Description, `SubmissionType`, RubricCriteria[], DueOffsetDays)
- `Badge` (Name, Icon, Description, CriteriaKind {Auto|Manual}, CompetencyId?)
- `Competency` (Name) + `ProgressionStage` (Order, Name, CompetencyIds[]) — the progression framework

**Learning activity / tracking** (the "replace the spreadsheet" data)

- `AssignedWork` (ItemType {Lesson|Module|Assignment}, ItemId, RecipientKind {Youth|Cohort}, RecipientId, DueDate, AssignedBy)
- `LessonProgress` (YouthId, LessonId, `ProgressStatus` {NotStarted=1, InProgress=2, Completed=3}, CompletedAt?)
- `QuizAttempt` (YouthId, QuizId, AnswersJson, Score, Passed, SubmittedAt) → drives quiz-avg
- `Submission` (YouthId, AssignmentId, Body, PhotoUrl?, `SubmissionStatus` {Submitted|UnderReview|Approved|NeedsRevision}, MentorComment?, ReviewedBy?)
- `ReflectionEntry` (YouthId, LessonId, AnswersJson, Mood, `Visibility` {Private|SharedWithMentor})
- `CompetencyAssessment` (YouthId, CompetencyId, `CompetencyLevel` {NotStarted=1, Developing=2, Proficient=3, Mastered=4}, AssessedBy, Notes, AssessedAt)
- `BadgeAward` (YouthId, BadgeId, AwardedAt, AwardedBy?)

**Engagement & operations**

- `Session` (CohortId, StartsAt, DurationMin, Location, MentorId, RecurrenceRule?) — calendar
- `AttendanceRecord` (SessionId, YouthId, `AttendanceStatus` {Present|Absent|Late|Excused}, RecordedBy, RecordedAt)
- `CheckIn` (YouthId, MentorId, Date, `CheckInType`, Engagement, GoalsDiscussed, PrayerRequests, ActionItems, `Visibility`, NextFollowUp?)
- `Flag` (YouthId, Reason, FlaggedBy, FlaggedAt, ResolvedAt?) — at-risk
- `Announcement` (Title, Body, AuthorId, `Audience` {Org|Cohort|Role}, AudienceRef?, ScheduledAt?, SentAt?)
- `MessageThread` (ParticipantIds[]) + `Message` (ThreadId, SenderId, Body, SentAt, ReadAt?)
- `Notification` (UserId, `NotificationType`, Title, Body, LinkPath, ReadAt?)
- `AuditEntry` (ActorId, Action, EntityType, EntityId, At, MetadataJson) — youth-safety accountability

**Reports** are read-model **queries** over the above (no new tables): progression-by-cohort, attendance rates, staff activity, individual youth report, cohort progress board.

---

## 5. Application layer (CQRS)

- **One request + one handler + optional validator per file**, suffixes `…Command/…Query → …CommandHandler/…QueryHandler → …CommandValidator`. Requests/DTOs/results are **records**; handlers are **classes** with constructor-injected `IMediator`/ports.
- **Ports (`Abstractions/`)** keep the layer infrastructure-free: `IAppDbContext` (exposes `DbSet<T>` + `SaveChangesAsync`), `IClock`, `ICurrentUser` (id/role from JWT), `IJwtTokenIssuer`, `IPasswordHasher`, `IRefreshTokenStore`, `IChangePublisher` (SignalR), `ISheetImporter`, `IReportRenderer`.
- **Pipeline:** a single `ValidationBehavior<TReq,TRes>` fans out `IValidator<TReq>` and throws `ValidationException`. (Optionally add an `AuditBehavior` for commands.)
- **Reads** use `.AsNoTracking()` + inline `.Select(x => new XDto(...))` projection; **writes** load, mutate, `SaveChangesAsync`, then `IChangePublisher.PublishAsync(...)`.
- `AddApplication()` scans the assembly for MediatR handlers + validators and registers the behavior.

```csharp
// Waypoint.Application/Progress/CompleteLessonCommand.cs
public record CompleteLessonCommand(Guid YouthId, Guid LessonId) : IRequest<LessonProgressDto>;

// Waypoint.Application/Progress/CompleteLessonCommandHandler.cs
public sealed class CompleteLessonCommandHandler(IAppDbContext db, IClock clock, IChangePublisher changes)
    : IRequestHandler<CompleteLessonCommand, LessonProgressDto>
{
    public async Task<LessonProgressDto> Handle(CompleteLessonCommand cmd, CancellationToken ct)
    {
        var progress = await db.LessonProgress
            .FirstOrDefaultAsync(p => p.YouthId == cmd.YouthId && p.LessonId == cmd.LessonId, ct)
            ?? db.LessonProgress.Add(new LessonProgress { YouthId = cmd.YouthId, LessonId = cmd.LessonId }).Entity;
        progress.Status = ProgressStatus.Completed;
        progress.CompletedAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);
        await changes.PublishAsync(new ProgressUpdated(cmd.YouthId, cmd.LessonId), ct);
        return new LessonProgressDto(progress.LessonId, progress.Status, progress.CompletedAt);
    }
}
```

---

## 6. API layer

- Thin `[ApiController]` controllers deriving straight from `ControllerBase`, one per resource area, attribute-routed `api/<plural>` with kebab sub-routes and typed constraints (`{id:guid}`). **`CancellationToken` threaded into every action.** Request DTOs are records in `Waypoint.Api/Contracts/`; the controller maps `Request → Command`.
- **Middleware order:** `SecurityHeaders → RequestLogging → ExceptionHandling(→ProblemDetails) → Auth → Controllers`.
- **OpenAPI:** Swashbuckle with JWT bearer security scheme; the generated `swagger.json` is the machine-readable side of the ICD (§12).
- **CORS:** allow the front-end origin(s); for the offline file:// prototype, a dev policy allowing `null`/localhost origins.
- **Versioning:** `api/v1/...` via `Asp.Versioning.Mvc` (optional but recommended before first release).

```csharp
[ApiController]
[Authorize]
[Route("api/cohorts")]
public class CohortsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}/progress")]                 // the Director's "progress board"
    [Authorize(Policy = Policies.StaffOnly)]
    public async Task<ActionResult<CohortProgressDto>> Progress(Guid id, CancellationToken ct)
        => Ok(await mediator.Send(new CohortProgressQuery(id), ct));
}
```

### Controller catalog (maps to the mockups)

`Auth · Me · Consent · People · Applications · Invitations · Cohorts · Youth(profile/journey/progress) · Tracks · Modules · Lessons · Quizzes · QuizAttempts · Assignments · Submissions · Reflections · Competencies · Progression · Badges · Sessions · Attendance · CheckIns · Flags · Announcements · Messages · Notifications · Reports · Audit · Import · OrgSettings`

---

## 7. Authentication & authorization (JWT OAuth2)

- **Scheme:** `AddAuthentication(JwtBearer).AddJwtBearer(...)` with full `TokenValidationParameters` (issuer/audience/lifetime/signing key, `ClockSkew = 30s`, `RequireHttpsMetadata` off only in Development). `JwtOptions` bound from config section `"Jwt"`.
- **Flows:** register, **invite-accept** (mentor/guardian/youth set password from an invite), **sign-in** (`OAuth2 password` grant → access + refresh), **refresh** (rotating family, reuse detection), **guardian consent** gate for minors, sign-out (revoke family), password reset. `ISignInThrottle` for lockout.
- **Tokens:** `IJwtTokenIssuer` (Application port) → `JwtTokenIssuer` (Infrastructure) using `System.IdentityModel.Tokens.Jwt`, HMAC-SHA256, claims `sub`, `email`, `role`, `jti`. Access token ~60 min; refresh long-lived, hashed at rest.
- **Passwords:** `IPasswordHasher` → `BCryptPasswordHasher` (`BCrypt.Net-Next 4.0.3`).
- **Authorization:** role claim + named policies in a `Policies` constant class:
  - `YouthOnly`, `StaffOnly` (Mentor+Director), `DirectorOnly`, `GuardianOnly`, `VolunteerOnly`, plus resource checks (a mentor may only read _their_ youth; a youth only their own data) via an `IAuthorizationHandler` or in-handler `ICurrentUser` checks.
- Roles: `UserRole { Youth=1, Mentor=2, Director=3, Guardian=4, Volunteer=5 }`.

_(Alternative if richer user management is wanted: ASP.NET Core Identity + policy-based auth, as in `appbuilder`. Adds Identity tables; keeps the same policy names. See §16.)_

---

## 8. Real-time (SignalR)

Handlers stay transport-agnostic: they call the Application port `IChangePublisher`; the Api supplies a SignalR adapter.

```
Waypoint.Application/Abstractions/IChangePublisher.cs      // port
Waypoint.Api/Realtime/WaypointHub.cs                         // public sealed class WaypointHub : Hub;  [Authorize]
Waypoint.Api/Realtime/SignalRChangePublisher.cs            // IChangePublisher adapter over IHubContext<WaypointHub>
```

- Register: `services.AddSignalR();`, `services.AddScoped<IChangePublisher, SignalRChangePublisher>();`, `app.MapHub<WaypointHub>("/hubs/waypoint").RequireAuthorization();`
- **Groups:** on connect, add the connection to `user-{id}`, `cohort-{cohortId}`, and `role-{role}` groups (from JWT claims in `OnConnectedAsync`).
- **Events (server→client):** `progressUpdated`, `quizGraded`, `submissionReviewed`, `badgeAwarded`, `attendanceSaved`, `newMessage`, `announcementPosted`, `youthFlagged`, `notificationCreated`. Payloads are records serialized as JSON (documented in the ICD).
- **Auth on the hub:** JWT via `access_token` query string (SignalR's standard) wired in `JwtBearerEvents.OnMessageReceived` for the `/hubs` path.
- Front-end maps: youth home "continue/notifications", mentor dashboard live queues, admin progress board and dashboard counters, messaging, attendance sync.

---

## 9. Persistence (EF Core)

- **`AppDbContext : DbContext, IAppDbContext`** in Infrastructure; handlers depend on `IAppDbContext`. `DbSet`s via `=> Set<T>()`.
- **Configuration:** inline `modelBuilder.Entity<T>(b => { … })` blocks in `OnModelCreating` (house style — not `IEntityTypeConfiguration` per entity): `HasMaxLength`, `HasIndex`, enum `HasConversion<int>()`, relationships, and `HasData` for reference/seed data (tracks, badges, competencies, the demo cohort).
- **Provider:** `UseSqlServer(connectionString)`; local `Server=.\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True`, prod via config/secret. Connection string key `DefaultConnection` with fail-fast.
- **Migrations:** `Waypoint.Infrastructure/Migrations/`; `Microsoft.EntityFrameworkCore.Design` referenced by Infrastructure **and** Api (Api is the EF startup project). Apply at startup with `await db.Database.MigrateAsync();` (and/or via the CLI `db migrate`).
- **Concurrency:** `rowversion` on mutable aggregates (attendance, submissions) to support the mockup's "unsaved changes / sync" flows.
- **Seeding:** a `DevDataSeeder` reproduces the mockups' sample cast (Jayden Alcantara, Marcus Bello, Shawn Peters, the three cohorts, the Servant Leadership module) so the API demos identically to the front-end.

---

## 10. Admin CLI — `way` (dotnet tool)

A `System.CommandLine` console app in `Waypoint.Cli`, packaged as a dotnet tool, that **reuses `AddApplication()` + `AddInfrastructure()`** through a Generic Host — so every admin action runs the same handlers/validation as the API.

```xml
<!-- Waypoint.Cli.csproj -->
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net8.0</TargetFramework>
  <AssemblyName>way</AssemblyName>
  <PackAsTool>true</PackAsTool>
  <ToolCommandName>way</ToolCommandName>
  <PackageId>Waypoint.Cli</PackageId>
  <Version>1.0.0</Version>
</PropertyGroup>
```

- **Version:** `System.CommandLine 2.0.0-beta4.22272.1` (matches `mvp`/`CodeGenerator.Cli`; uses `Host.CreateDefaultBuilder` + `RootCommand.InvokeAsync` + `SetHandler`). _Forward option:_ GA `2.0.x` — but it drops `System.CommandLine.Hosting` and replaces `SetHandler` with `SetAction(ParseResult)`, so DI must be wired by hand. Stay on beta4 for house consistency on .NET 8.²
- **One command per file**, `static class XCommand { public static Command Create(IServiceProvider sp) }`, resolving handlers from the host container.

**Command tree:**

```
way
  db     migrate | seed | reset
  user   create | list | set-role | reset-password | deactivate
  invite create | resend | revoke | list
  cohort create | assign-mentor | add-youth | progress <id>
  import youth   --from <csv>        # Google Sheets migration (mockup: admin/data-import)
  report youth <id> --format pdf|csv     |  report cohort <id>
  badge  award <youthId> <badgeId>
  token  issue --user <email>        # dev/testing helper
```

This covers the requested "admin utility functions" and the Sheets-migration path the org needs off spreadsheets.

---

## 11. Testing (NUnit)

Two projects; **NUnit 4.x** with `NUnit3TestAdapter` + `Microsoft.NET.Test.Sdk`. We keep the house **patterns** (feature-foldered, driven through the real API, real SQL DB per fixture, hand-rolled fakes, traceability comments) and translate xUnit's `IAsyncLifetime` to NUnit's async `[OneTimeSetUp]/[OneTimeTearDown]`. **Every acceptance (integration) test must trace to an L2 requirement** — see "Requirements traceability" below; this is a hard rule, enforced in CI.

**`Waypoint.UnitTests`** — fast, no DB:

- Handler tests against fakes/in-memory `IAppDbContext` (SQLite-in-memory or a hand-rolled fake), validator tests, domain rule tests, JWT issuer, password hasher, progression/quiz-scoring logic.
- `Moq 4.20.x` allowed for ports, but prefer hand-rolled fakes (e.g. `FakeClock`) as the house does.

**`Waypoint.IntegrationTests`** — through the HTTP surface:

- `Microsoft.AspNetCore.Mvc.Testing` + `WebApplicationFactory<Program>` (add `public partial class Program;` to `Program.cs`).
- **Real SQL Server, one throwaway DB per test fixture:** a `SqlServerFixture` (base `[TestFixture]` or a `[SetUpFixture]`) creates `Waypoint_IntegrationTests_{Guid:N}` in `[OneTimeSetUp]` (`MigrateAsync`), `DROP DATABASE` in `[OneTimeTearDown]`; connection overridable via env var `WAYPOINT_TEST_SQLSERVER` (default `.\SQLEXPRESS`). The factory swaps the `DbContextOptions` in `ConfigureTestServices`. _(Optional: Respawn for reset-between-tests, or Testcontainers for CI without a local instance.)_
- SignalR is tested with `Microsoft.AspNetCore.SignalR.Client` (`HubConnectionBuilder`) asserting events fire (e.g. quiz submit ⇒ `quizGraded`).
- Feature-foldered (`Auth/`, `Cohorts/`, `Progress/`…), classes `…Tests`, methods `snake_case_sentences`, each **tagged with the L2 requirement it verifies** — a `// Traces to: L2-<AREA>-NNN` comment _and_ a machine-readable `[Property("Req", "L2-<AREA>-NNN")]` attribute (see traceability rule below).
- Coverage via `coverlet.collector`; gate meaningful handler/endpoint coverage in CI.

### Requirements traceability — every acceptance test → an L2 requirement (mandatory)

**Rule: every acceptance test MUST trace to a Level-2 (L2) requirement.** No exceptions.

- **What an L2 requirement is:** a detailed, individually-testable requirement with explicit **acceptance criteria**, authored via the requirements process into **`docs/specs/`**, each with a stable id `L2-<AREA>-NNN` (e.g. `L2-AUTH-004`, `L2-PROGRESS-012`). L2 sits under the L1 high-level requirements. The acceptance test is the **executable form of that requirement's acceptance criteria**.
- **ATDD source:** the failing acceptance test written in step 1 of the ATDD loop (see `CLAUDE.md`) is derived **directly from an L2 requirement's acceptance criteria**. Therefore: **no acceptance test may be written without a governing L2 requirement, and no L2 requirement is "done" until a passing acceptance test covers it.** If an L2 requirement does not yet exist for the behaviour, author it first.
- **How the link is recorded:** each acceptance-test class/method cites its L2 id(s) both as a `// Traces to: L2-<AREA>-NNN` comment **and** as an NUnit `[Property("Req", "L2-<AREA>-NNN")]` attribute (repeatable) so the link is machine-readable, not just prose.
- **Bidirectional coverage (both directions enforced):**
  - _Forward_ — every L2 requirement in `docs/specs/` has **≥1** acceptance test tagged with its id.
  - _Backward_ — every acceptance test carries **≥1 valid** `L2-*` id that exists in `docs/specs/`.
- **CI gate:** a traceability check reads the `[Property("Req", …)]` metadata across the `Waypoint.IntegrationTests` assembly and reconciles it against the `docs/specs/` L2 catalog. The build **fails** if any acceptance test has no valid L2 id, or any L2 requirement has zero covering acceptance tests. A generated **traceability matrix** (L2 id ↔ tests, with pass/fail) is published as a build artifact each run.
- Unit tests _may_ also cite L2 ids but are not required to — the **L2 ↔ acceptance-test** link is the enforced contract.

---

## 12. Interface Control Document (ICD)

Two layers, kept in sync:

1. **Machine-readable:** the live **OpenAPI/Swagger** doc (`/swagger/v1/swagger.json`) — the source of truth for request/response schemas.
2. **Human ICD** in `docs/icd/` — the reviewable interface contract for front-end and integrators. Structure:
   - `00-overview.md` — base URL, versioning, auth model, error model (`ProblemDetails`), pagination, date/enum encoding.
   - `01-auth.md` — token flows (sequence diagrams), grant types, refresh rotation.
   - `10-rest-endpoints.md` — endpoint catalog: method, route, auth policy, request DTO, response DTO, status codes, per area.
   - `20-signalr.md` — hub URL, connection auth, authoritative per-dispatch targeting model, event catalog with payload schemas.
   - `30-dtos.md` — shared DTO/enum schemas (or link to OpenAPI components).
   - `40-errors.md` — `ProblemDetails` types + validation error shape.

Representative slice (the Director's progress board):

| Method  | Route                          | Policy        | Request                   | Response                             | Codes              |
| ------- | ------------------------------ | ------------- | ------------------------- | ------------------------------------ | ------------------ |
| `GET`   | `/api/cohorts/{id}/progress`   | StaffOnly     | –                         | `CohortProgressDto`                  | 200, 401, 403, 404 |
| `POST`  | `/api/quizzes/{id}/attempts`   | YouthOnly     | `SubmitQuizRequest`       | `QuizResultDto`                      | 201, 400, 403      |
| `PATCH` | `/api/submissions/{id}/review` | StaffOnly     | `ReviewSubmissionRequest` | `SubmissionDto`                      | 200, 400, 403, 404 |
| SignalR | `/hubs/waypoint` → `quizGraded`  | authenticated | –                         | `{ youthId, quizId, score, passed }` | –                  |

_Generation:_ the ICD markdown can be produced/refreshed with the repo's **`public-icd`** skill (adds C4/sequence/class PlantUML), seeded from the OpenAPI output so it never drifts from the code.

---

## 13. Cross-cutting concerns

- **Logging:** structured JSON console logging + `RequestLoggingMiddleware`; redact secrets. (Serilog optional.)
- **Config/secrets:** `appsettings.json` + environment + user-secrets in dev; connection string & `Jwt:SigningKey` never committed.
- **Security headers** middleware; HTTPS redirection; `RequireHttpsMetadata` in prod.
- **Health checks:** `/health` (liveness) + `/health/ready` (DB).
- **Rate limiting:** ASP.NET `RateLimiter` on auth endpoints.
- **Audit:** an `AuditBehavior`/interceptor writes `AuditEntry` for mutating commands (youth-safety accountability the mockups imply).

---

## 14. Delivery milestones

Each milestone is independently shippable and lights up specific front-end screens. **Every milestone begins by authoring/refreshing its L2 requirements in `docs/specs/`, then writing the failing acceptance tests that trace to them (ATDD) — no milestone's implementation starts before its L2 requirements exist.** A milestone is "done" only when every one of its L2 requirements has a passing, traced acceptance test and the CI traceability gate is green.

1. **M0 — Foundation.** Solution, `Directory.Build.props`, 4 projects + 2 test projects, `AppDbContext`, `AddApplication`/`AddInfrastructure`, ProblemDetails middleware, health checks, Swagger, CI build+test. **Stand up `docs/specs/` (L1/L2) and the CI traceability gate** that reconciles `[Property("Req", …)]` tags against the L2 catalog. _(Proves the skeleton with one trivial vertical slice whose NUnit acceptance test traces to its L2 id.)_
2. **M1 — Auth & people.** Users/roles, JWT + refresh, invites, guardian consent, applications intake. Screens: `auth/*`, `admin/people`, `admin/invitations`, `admin/applications`.
3. **M2 — Curriculum.** Tracks/Modules/Lessons/Quizzes/Assignments + builders. Screens: `admin/tracks|module-editor|lesson-editor|quiz-builder|assignment-builder`, `youth/curriculum|module|lesson|lesson-bible`.
4. **M3 — Progress & tracking (the spreadsheet-killer).** Lesson completion, quiz attempts, submissions+review, reflections, competencies, progression, badges, cohort progress board. Screens: `youth/quiz*|submit-work|reflection`, `mentor/youth-detail|competency|reviews|grade-submission`, `admin/cohort-progress`.
5. **M4 — Engagement.** Sessions/calendar, attendance (+offline sync semantics), check-ins, flags, announcements, messages, notifications. Screens: `mentor/attendance|check-in|flags|calendar|messages`, `youth/schedule|messages|announcements|notifications`.
6. **M5 — Real-time.** SignalR hub + `IChangePublisher`, wire events into M3/M4 handlers, client reconnection semantics.
7. **M6 — Reports & Admin CLI.** Report queries + rendering; `way` dotnet tool incl. Google-Sheets import. Screens: `admin/reports|report-*|data-import`.
8. **M7 — Hardening & ICD.** Authorization edge cases, rate limiting, audit, load/security pass, finalize `docs/icd/`, package the CLI, deployment runbook.

---

## 15. Package versions (target)

| Package                                                 | Version             | Project             |
| ------------------------------------------------------- | ------------------- | ------------------- |
| `MediatR`                                               | **12.5.0**          | Application         |
| `FluentValidation` (+ `.DependencyInjectionExtensions`) | 11.10.0             | Application         |
| `Microsoft.EntityFrameworkCore.SqlServer`               | 8.0.x               | Infrastructure      |
| `Microsoft.EntityFrameworkCore.Design`                  | 8.0.x               | Infrastructure, Api |
| `Microsoft.AspNetCore.Authentication.JwtBearer`         | 8.0.x               | Api                 |
| `System.IdentityModel.Tokens.Jwt`                       | 8.x                 | Infrastructure      |
| `BCrypt.Net-Next`                                       | 4.0.3               | Infrastructure      |
| `Swashbuckle.AspNetCore`                                | 6.6.x               | Api                 |
| `Microsoft.AspNetCore.SignalR`                          | (built-in)          | Api                 |
| `System.CommandLine`                                    | 2.0.0-beta4.22272.1 | Cli                 |
| `Microsoft.Extensions.Hosting`                          | 8.0.x               | Cli                 |
| `NUnit`                                                 | 4.x                 | Tests               |
| `NUnit3TestAdapter`                                     | 4.x                 | Tests               |
| `Microsoft.NET.Test.Sdk`                                | 17.x                | Tests               |
| `Microsoft.AspNetCore.Mvc.Testing`                      | 8.0.x               | IntegrationTests    |
| `Microsoft.Data.SqlClient`                              | 5.2.x               | IntegrationTests    |
| `Microsoft.AspNetCore.SignalR.Client`                   | 8.0.x               | IntegrationTests    |
| `Moq` _(optional)_                                      | 4.20.x              | UnitTests           |
| `coverlet.collector`                                    | 6.x                 | Tests               |

---

## 16. Open decisions (confirm before M0)

1. **Error model:** typed exceptions + `ProblemDetails` middleware (Forge, recommended) **vs** `Result<T>` + `ToActionResult` (appbuilder). _Recommendation: exceptions + ProblemDetails_ (cleaner handlers; matches the canonical template).
2. **Auth stack:** hand-rolled JWT via `IJwtTokenIssuer` (Forge, recommended for control) **vs** ASP.NET Core Identity + policies (if you want built-in user management/2FA later).
3. **NUnit vs house xUnit:** plan honors the requested **NUnit** — confirm you want to diverge from the house xUnit standard for this repo.
4. **Test DB:** real SQL Express per fixture (house pattern, needs SQL Express on dev/CI) **vs** Testcontainers (portable CI, needs Docker) **vs** SQLite for integration (fast but not SQL-Server-faithful). _Recommendation: SQL Express per fixture locally, Testcontainers in CI._
5. **System.CommandLine:** beta4 to match existing tools (recommended) **vs** GA 2.0.x (current API, manual DI).
6. **Repo layout:** backend under `waypoint/backend/` (recommended) **vs** a separate `waypoint-api` repo.
7. **MediatR:** pin 12.5.0 (recommended) **vs** adopt v13 Community edition (free for this nonprofit, latest features, RPL-1.5 copyleft).

---

### Sources

- MediatR licensing — [Jimmy Bogard: Licensing Update](https://www.jimmybogard.com/automapper-and-mediatr-licensing-update/), [Commercial launch discussion](https://github.com/LuckyPennySoftware/MediatR/discussions/1123), [Milan Jovanović: going commercial](https://milanjovanovic.tech/blog/mediatr-and-masstransit-going-commercial-what-this-means-for-you)
- System.CommandLine — [GA path & beta5+](https://github.com/dotnet/command-line-api/issues/2576), [Migration guide 2.0.0-beta5+](https://learn.microsoft.com/en-us/dotnet/standard/commandline/migration-guide-2.0.0-beta5)
