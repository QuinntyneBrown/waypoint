using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Waypoint.Api.Authorization;
using Waypoint.Api.Middleware;
using Waypoint.Api.Observability;
using Waypoint.Api.Realtime;
using Waypoint.Api.Security;
using Waypoint.Api.TestSupport;
using Waypoint.Application;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Authentication.Auth;
using Waypoint.Application.Shared.Common;
using Waypoint.Infrastructure;
using Waypoint.Infrastructure.Health;

var builder = WebApplication.CreateBuilder(args);

// The global handler emits a deliberately redacted event. Suppress the framework's duplicate event,
// which otherwise includes the raw exception message and stack trace.
builder.Logging.AddFilter("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogLevel.None);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.RemoveAll<IEmailSender>();
    builder.Services.AddSingleton<Waypoint.Api.TestSupport.TestingEmailSender>();
    builder.Services.AddSingleton<IEmailSender>(serviceProvider =>
        serviceProvider.GetRequiredService<Waypoint.Api.TestSupport.TestingEmailSender>());
}

builder.Services.AddControllers();
builder.Services.AddSingleton<IExceptionAlertSink, LoggingExceptionAlertSink>();
builder.Services.AddEndpointsApiExplorer();

// Near-real-time delivery hub + its Application-facing port (L2-NFR-005).
builder.Services.AddSignalR();
builder.Services.AddSingleton<RealtimeConnectionRegistry>();
builder.Services.AddSingleton<IChangePublisher, SignalRChangePublisher>();

// Liveness (/health, no checks) + readiness (/health/ready, DB reachability) — L2-NFR-011 AC1.
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseReadyHealthCheck>("database", tags: new[] { "ready" });
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Waypoint API", Version = "v1" });
    // Namespace-qualified schema ids so same-named DTOs in different features don't collide.
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
    };
    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
});

builder.Services.AddCors(options =>
{
    var allowedCorsOrigins = SecurityBoundaryConfiguration.GetAllowedCorsOrigins(
        builder.Configuration,
        builder.Environment);
    options.AddDefaultPolicy(policy =>
    {
        if (allowedCorsOrigins.Length > 0)
        {
            policy.WithOrigins(allowedCorsOrigins);
        }
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("X-Waypoint-State")
            .AllowCredentials();
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// Bind bearer validation from JwtOptions at runtime so the signing key matches the token issuer
// (and honours test/config overrides applied after the builder is created).
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        // Browser websocket clients cannot send an Authorization header, so accept the token from the
        // query string for the SignalR hub path only (L2-NFR-005 AC2).
        bearer.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var subject = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = context.Principal?.FindFirst(ClaimTypes.Role)?.Value;
                var sessionVersion = context.Principal?.FindFirst("session_version")?.Value;
                if (!Guid.TryParse(subject, out var userId)
                    || !int.TryParse(sessionVersion, out var tokenSessionVersion))
                {
                    context.Fail("The session is no longer valid.");
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<IAppDbContext>();
                var user = await db.Users
                    .AsNoTracking()
                    .Where(item => item.Id == userId)
                    .Select(item => new { item.Role, item.Status, item.SessionVersion })
                    .FirstOrDefaultAsync(context.HttpContext.RequestAborted);
                if (user is null
                    || user.Status != UserStatus.Active
                    || user.Role != role
                    || user.SessionVersion != tokenSessionVersion)
                {
                    context.Fail("The session is no longer valid.");
                }
            },
        };
    });

builder.Services.AddAuthorization(options =>
{
    // L2-AUTH-009 permission matrix (role checks; resource scoping is enforced in handlers).
    options.AddPolicy(Policies.YouthParticipation, policy =>
    {
        policy.RequireRole(Roles.Youth);
        policy.AddRequirements(new YouthParticipationRequirement());
    });
    options.AddPolicy(Policies.ManageUsers, p => p.RequireRole(Roles.ProgramAdministrator));
    options.AddPolicy(Policies.ApproveApplications, p => p.RequireRole(Roles.ProgramAdministrator));
    options.AddPolicy(Policies.EditCurriculum, p => p.RequireRole(Roles.ProgramAdministrator));
    options.AddPolicy(Policies.ImportExportData, p => p.RequireRole(Roles.ProgramAdministrator));
    options.AddPolicy(Policies.ViewAuditLog, p => p.RequireRole(Roles.ProgramAdministrator));
    options.AddPolicy(Policies.InviteUsers, p => p.RequireRole(Roles.ProgramAdministrator, Roles.Mentor));
    options.AddPolicy(Policies.RecordAttendance, p => p.RequireRole(Roles.ProgramAdministrator, Roles.Mentor, Roles.Volunteer));
});
builder.Services.AddScoped<IAuthorizationHandler, YouthParticipationAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, WaypointAuthorizationResultHandler>();

// Rate limiting on the auth surface (L2-AUTH-001 AC5 / L2-NFR-002). Disabled under test so the
// acceptance suite is deterministic.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddRateLimiter(options =>
    {
        var sensitiveRateLimit = SecurityBoundaryConfiguration.GetSensitiveRateLimit(builder.Configuration);
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = async (rejected, _) =>
        {
            if (rejected.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                rejected.HttpContext.Response.Headers.RetryAfter =
                    Math.Ceiling(retryAfter.TotalSeconds).ToString();
            }
            await Results.Problem(
                title: "Too many requests. Please try again later.",
                statusCode: StatusCodes.Status429TooManyRequests)
                .ExecuteAsync(rejected.HttpContext);
        };
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            if (!SecurityBoundaryConfiguration.IsSensitiveEndpoint(context.Request.Path))
            {
                return RateLimitPartition.GetNoLimiter("unlimited");
            }

            var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter($"sensitive:{key}", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = sensitiveRateLimit.PermitLimit,
                Window = TimeSpan.FromSeconds(sensitiveRateLimit.WindowSeconds),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true,
            });
        });
    });
}

var app = builder.Build();
InfrastructureStartupValidator.Validate(app.Configuration, app.Environment.EnvironmentName);

// Establish one validated correlation id before any response or exception path is handled.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// First in the pipeline so security headers cover every response, including the exception (500) and
// maintenance (503) short-circuits below (L2-NFR-002 AC3).
app.UseMiddleware<SecurityHeadersMiddleware>();

if (app.Environment.IsProduction())
{
    app.UseMiddleware<RequireHttpsMiddleware>();
}

app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var ex = feature?.Error;
        // ExceptionHandler clears the original response before running this branch, so restore the
        // correlation header established by CorrelationIdMiddleware.
        context.Response.Headers[CorrelationIdMiddleware.HeaderName] = context.TraceIdentifier;

        if (ex is ValidationException vex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Validation failed",
                errors = vex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }),
            });
            return;
        }

        if (ex is InvalidCredentialsException || ex is EmailAlreadyRegisteredException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { title = ex.Message });
            return;
        }

        if (ex is AppException appEx)
        {
            if (appEx.StatusCode == StatusCodes.Status403Forbidden)
            {
                await DeniedActionAudit.RecordAsync(context);
            }
            context.Response.StatusCode = appEx.StatusCode;
            if (appEx is MediaConsentRequiredException)
            {
                context.Response.Headers["X-Waypoint-State"] = "media-consent-required";
            }
            await context.Response.WriteAsJsonAsync(new { title = appEx.Message });
            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        var correlationId = context.TraceIdentifier;
        var exceptionType = ex?.GetType().Name ?? "UnknownException";
        var route = SafeRequestTelemetry.Route(context);
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Waypoint.Api.UnhandledException");
        logger.LogError(
            "Unhandled API exception. CorrelationId={CorrelationId} ExceptionType={ExceptionType} " +
            "Method={Method} Route={Route}",
            correlationId,
            exceptionType,
            context.Request.Method,
            route);
        await context.RequestServices.GetRequiredService<IExceptionAlertSink>().NotifyAsync(
            exceptionType,
            correlationId,
            context.Request.Method,
            route);
        await context.Response.WriteAsJsonAsync(new
        {
            title = "An unexpected error occurred.",
            correlationId,
        });
    });
});

// Short-circuit to a 503 state while maintenance mode is on (L2-PLATFORM-009).
app.UseMiddleware<MaintenanceMiddleware>();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseRateLimiter();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<RealtimeHub>("/hubs/realtime", options =>
{
    // Stop long-lived transports at JWT expiry; the publisher also rejects an expired registry
    // snapshot so no passive event can leak during disconnect propagation.
    options.CloseOnAuthenticationExpiration = true;
});

// Liveness: process is up (no dependency checks). Readiness: dependencies (DB) reachable. L2-NFR-011 AC1.
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready") });

// Diagnostics hook (never mapped in Production) used to verify the global 500 contract
// (friendly body, no stack trace) — L2-PLATFORM-008.
if (!app.Environment.IsProduction())
{
    var diagnosticMessage = app.Environment.IsEnvironment("Testing")
        ? "Intentional diagnostic failure for diagnostic.person@example.test using Bearer diagnostic-secret-token."
        : "Intentional diagnostic failure.";
    app.MapGet("/api/_diag/throw", () =>
    {
        throw new InvalidOperationException(diagnosticMessage);
    });
}

// Test-support seeding (Testing environment ONLY, never Production/Development). The E2E acceptance
// harness has HTTP access only, so this lets it provision active non-Youth identities (Program
// Administrator / Mentor / Guardian / Volunteer) that otherwise require in-process DbContext seeding.
// Tests then drive the real product APIs as that identity.
if (app.Environment.IsEnvironment("Testing"))
{
    app.MapTestSupportEndpoints();
}

await using (var migrationScope = app.Services.CreateAsyncScope())
{
    await migrationScope.ServiceProvider.GetRequiredService<IDatabaseLifecycle>().MigrateAsync();
}

app.Run();

public partial class Program { }
