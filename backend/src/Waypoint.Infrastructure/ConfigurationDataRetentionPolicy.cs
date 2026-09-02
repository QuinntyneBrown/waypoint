using Microsoft.Extensions.Configuration;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

public sealed class ConfigurationDataRetentionPolicy : IDataRetentionPolicy
{
    public int DeactivatedUserRetentionDays { get; }

    public ConfigurationDataRetentionPolicy(IConfiguration configuration)
    {
        DeactivatedUserRetentionDays = configuration.GetValue("Privacy:DeactivatedUserRetentionDays", 365);
        if (DeactivatedUserRetentionDays is < 1 or > 3650)
        {
            throw new InvalidOperationException(
                "Privacy:DeactivatedUserRetentionDays must be between 1 and 3650.");
        }
    }
}
