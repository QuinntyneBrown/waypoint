using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.ConsentAndSafeguarding;

public sealed class GuardianContactMissingException : AppException
{
    public GuardianContactMissingException()
        : base(400, "No guardian email is linked to this account. Please contact the program team.") { }
}

public sealed class ConsentRequestLinkInvalidException : AppException
{
    public ConsentRequestLinkInvalidException()
        : base(410, "This consent link has expired or has already been used. Ask the youth to send a new link.") { }
}
