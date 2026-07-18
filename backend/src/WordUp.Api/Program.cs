using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WordUp.Api.Authorization;
using WordUp.Api.Middleware;
using WordUp.Api.Realtime;
using WordUp.Application;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Authentication.Auth;
using WordUp.Application.Shared.Common;
using WordUp.Infrastructure;
using WordUp.Infrastructure.Health;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Near-real-time delivery hub + its Application-facing port (L2-NFR-005).
builder.Services.AddSignalR();
builder.Services.AddSingleton<IChangePublisher, SignalRChangePublisher>();

// Liveness (/health, no checks) + readiness (/health/ready, DB reachability) — L2-NFR-011 AC1.
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseReadyHealthCheck>("database", tags: new[] { "ready" });
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Word Up API", Version = "v1" });
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
    options.AddDefaultPolicy(policy => policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
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
        };
    });

builder.Services.AddAuthorization(options =>
{
    // L2-AUTH-009 permission matrix (role checks; resource scoping is enforced in handlers).
    options.AddPolicy(Policies.ManageUsers, p => p.RequireRole(Roles.ProgramAdministrator));
    options.AddPolicy(Policies.ApproveApplications, p => p.RequireRole(Roles.ProgramAdministrator));
    options.AddPolicy(Policies.EditCurriculum, p => p.RequireRole(Roles.ProgramAdministrator));
    options.AddPolicy(Policies.ImportExportData, p => p.RequireRole(Roles.ProgramAdministrator));
    options.AddPolicy(Policies.ViewAuditLog, p => p.RequireRole(Roles.ProgramAdministrator));
    options.AddPolicy(Policies.InviteUsers, p => p.RequireRole(Roles.ProgramAdministrator, Roles.Mentor));
    options.AddPolicy(Policies.RecordAttendance, p => p.RequireRole(Roles.ProgramAdministrator, Roles.Mentor, Roles.Volunteer));
});

// Rate limiting on the auth surface (L2-AUTH-001 AC5 / L2-NFR-002). Disabled under test so the
// acceptance suite is deterministic.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            if (!context.Request.Path.StartsWithSegments("/api/auth"))
            {
                return RateLimitPartition.GetNoLimiter("unlimited");
            }

            var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
            });
        });
    });
}

var app = builder.Build();

// First in the pipeline so security headers cover every response, including the exception (500) and
// maintenance (503) short-circuits below (L2-NFR-002 AC3).
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var ex = feature?.Error;

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
            context.Response.StatusCode = appEx.StatusCode;
            await context.Response.WriteAsJsonAsync(new { title = appEx.Message });
            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { title = "An unexpected error occurred." });
    });
});

// Short-circuit to a 503 state while maintenance mode is on (L2-PLATFORM-009).
app.UseMiddleware<MaintenanceMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseRateLimiter();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<RealtimeHub>("/hubs/realtime");

// Liveness: process is up (no dependency checks). Readiness: dependencies (DB) reachable. L2-NFR-011 AC1.
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready") });

// Diagnostics hook (never mapped in Production) used to verify the global 500 contract
// (friendly body, no stack trace) — L2-PLATFORM-008.
if (!app.Environment.IsProduction())
{
    app.MapGet("/api/_diag/throw", () =>
    {
        throw new InvalidOperationException("Intentional diagnostic failure.");
    });
}

// Test-support seeding (Testing environment ONLY, never Production/Development). The E2E acceptance
// harness has HTTP access only, so this lets it provision active non-Youth identities (Program
// Administrator / Mentor / Guardian / Volunteer) that otherwise require in-process DbContext seeding.
// Tests then drive the real product APIs as that identity.
if (app.Environment.IsEnvironment("Testing"))
{
    app.MapPost("/api/_test/users", async (
        WordUp.Api.Contracts.TestSupport.SeedUserBody body,
        IAppDbContext db,
        IPasswordHasher hasher,
        CancellationToken cancellationToken) =>
    {
        if (!Roles.IsValid(body.Role))
        {
            return Results.BadRequest(new { title = $"Unknown role '{body.Role}'." });
        }

        var user = new User
        {
            Email = body.Email,
            DisplayName = body.DisplayName,
            Role = body.Role,
            Status = UserStatus.Active,
            PasswordHash = hasher.Hash(body.Password),
            OnboardingCompleted = true,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return Results.Created($"/api/_test/users/{user.Id}", new { userId = user.Id });
    });
}

app.Run();

public partial class Program { }
