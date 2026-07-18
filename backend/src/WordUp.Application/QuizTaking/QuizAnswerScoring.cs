using System.Text.Json;

namespace WordUp.Application.QuizTaking;

/// <summary>Shared JSON helpers for quiz answers and options.</summary>
internal static class QuizAnswerScoring
{
    /// <summary>Parses a youth's answers JSON of the form { "questionId": [0, 1], ... }.</summary>
    public static Dictionary<Guid, HashSet<int>> ParseAnswers(string? json)
    {
        var result = new Dictionary<Guid, HashSet<int>>();
        if (string.IsNullOrWhiteSpace(json))
        {
            return result;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return result;
            }

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (Guid.TryParse(prop.Name, out var questionId))
                {
                    result[questionId] = ToIntSet(prop.Value);
                }
            }
        }
        catch (JsonException)
        {
            // Malformed answers score as no answers.
        }

        return result;
    }

    /// <summary>Parses a stored CorrectAnswersJson int array (e.g. [0] or [1, 2]).</summary>
    public static HashSet<int> ParseIntSet(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new HashSet<int>();
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            return ToIntSet(doc.RootElement);
        }
        catch (JsonException)
        {
            return new HashSet<int>();
        }
    }

    public static IReadOnlyList<string> ParseOptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    private static HashSet<int> ToIntSet(JsonElement element)
    {
        var set = new HashSet<int>();
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Number && item.TryGetInt32(out var n))
                {
                    set.Add(n);
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var single))
        {
            set.Add(single);
        }

        return set;
    }
}
