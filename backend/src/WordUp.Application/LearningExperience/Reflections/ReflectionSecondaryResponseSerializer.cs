using System.Text.Json;

namespace WordUp.Application.LearningExperience.Reflections;

internal static class ReflectionSecondaryResponseSerializer
{
    public static string? Serialize(IReadOnlyDictionary<string, string>? responses)
    {
        if (responses is null) return null;

        var normalized = new Dictionary<string, string>();
        foreach (var (prompt, response) in responses)
        {
            if (string.IsNullOrWhiteSpace(prompt) || string.IsNullOrWhiteSpace(response)) continue;
            normalized[prompt.Trim()] = response.Trim();
        }

        return normalized.Count == 0 ? null : JsonSerializer.Serialize(normalized);
    }

    public static IReadOnlyDictionary<string, string> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, string>();
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                ?? new Dictionary<string, string>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>();
        }
    }
}
