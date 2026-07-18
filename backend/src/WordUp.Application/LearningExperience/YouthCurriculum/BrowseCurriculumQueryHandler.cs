using MediatR;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.YouthCurriculum;

public class BrowseCurriculumQueryHandler : IRequestHandler<BrowseCurriculumQuery, CurriculumView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public BrowseCurriculumQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CurriculumView> Handle(BrowseCurriculumQuery request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException();

        var modules = await CurriculumProgressCalculator.BuildModuleStatusesAsync(
            _db, request.TrackId, youthId, cancellationToken);

        return new CurriculumView(modules);
    }
}
