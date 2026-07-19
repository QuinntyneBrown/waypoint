using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.ConsentAndSafeguarding;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Api.Authorization;

public sealed class YouthParticipationRequirement : IAuthorizationRequirement;

public sealed class YouthParticipationAuthorizationHandler
    : AuthorizationHandler<YouthParticipationRequirement>
{
    public const string ConsentRequiredItem = "WordUp.Authorization.ConsentRequired";

    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public YouthParticipationAuthorizationHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        YouthParticipationRequirement requirement)
    {
        var httpContext = context.Resource as HttpContext;
        var cancellationToken = httpContext?.RequestAborted ?? CancellationToken.None;
        if (_currentUser.UserId is not { } userId)
        {
            return;
        }

        var user = await _db.Users
            .AsNoTracking()
            .Where(item => item.Id == userId && item.Role == Roles.Youth)
            .Select(item => new { item.GuardianUserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return;
        }

        var allowed = await ConsentPolicyEvaluator.IsYouthParticipationAllowedAsync(
            _db,
            userId,
            user.GuardianUserId,
            _clock.UtcNow,
            cancellationToken);

        if (allowed)
        {
            context.Succeed(requirement);
            return;
        }

        if (httpContext is not null)
        {
            httpContext.Items[ConsentRequiredItem] = true;
        }
    }
}

public sealed class WordUpAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _fallback = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden)
        {
            await DeniedActionAudit.RecordAsync(context);
        }

        if (authorizeResult.Forbidden
            && context.Items.ContainsKey(YouthParticipationAuthorizationHandler.ConsentRequiredItem))
        {
            context.Response.Headers["X-WordUp-State"] = "consent-required";
            await Results.Problem(
                title: "Guardian participation consent is required.",
                statusCode: StatusCodes.Status403Forbidden)
                .ExecuteAsync(context);
            return;
        }

        await _fallback.HandleAsync(next, context, policy, authorizeResult);
    }
}
