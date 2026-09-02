using Microsoft.Extensions.Configuration;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

public sealed class ConfigurationAtRiskThresholds : IAtRiskThresholds
{
    public const string InactiveDaysKey = "AtRiskThresholds:InactiveDays";
    public const string MissedSessionsKey = "AtRiskThresholds:MissedSessions";
    public const string ProgressGapKey = "AtRiskThresholds:ProgressBelowCohortPercentage";

    private readonly IConfiguration _configuration;

    public ConfigurationAtRiskThresholds(IConfiguration configuration) => _configuration = configuration;

    public int InactiveDays => PositiveInt(InactiveDaysKey, 7);
    public int MissedSessions => PositiveInt(MissedSessionsKey, 1);
    public double ProgressBelowCohortPercentage => PositiveDouble(ProgressGapKey, 15);

    private int PositiveInt(string key, int defaultValue)
    {
        var configured = _configuration.GetValue<int?>(key);
        return configured is > 0 ? configured.Value : defaultValue;
    }

    private double PositiveDouble(string key, double defaultValue)
    {
        var configured = _configuration.GetValue<double?>(key);
        return configured is > 0 ? configured.Value : defaultValue;
    }
}
