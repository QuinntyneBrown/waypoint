using MediatR;

namespace WordUp.Application.PlatformServices.Access;

/// <summary>
/// Resolves the youth participation gate for the signed-in youth (L2-PLATFORM-011):
/// program access is only granted once the youth is placed in a cohort AND guardian consent is on file.
/// </summary>
public record AccessStateQuery() : IRequest<AccessState>;

public record AccessState(bool HasCohort, bool ConsentRequired, bool ParticipationAllowed);
