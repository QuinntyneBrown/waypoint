using System.Text;

namespace WordUp.Application.ReportingAndData.DataTransfer;

/// <summary>
/// Minimal, dependency-free CSV read/write helper (L2-REPORT-003/004). Handles comma-separated fields
/// with basic double-quote quoting/escaping ("" for a literal quote). Deliberately tiny — no NuGet package.
/// </summary>
internal static class Csv
{
    /// <summary>Serialises one row, quoting any field that contains a comma, quote, or newline.</summary>
    public static string ToRow(IEnumerable<string> fields) =>
        string.Join(",", fields.Select(Quote));

    private static string Quote(string? field)
    {
        field ??= string.Empty;
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }

        return field;
    }

    /// <summary>Parses CSV content into rows of fields, honouring double-quoted fields.</summary>
    public static IReadOnlyList<IReadOnlyList<string>> Parse(string content)
    {
        var rows = new List<IReadOnlyList<string>>();
        if (string.IsNullOrEmpty(content))
        {
            return rows;
        }

        var field = new StringBuilder();
        var current = new List<string>();
        var inQuotes = false;
        var i = 0;

        while (i < content.Length)
        {
            var c = content[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < content.Length && content[i + 1] == '"')
                    {
                        field.Append('"');
                        i += 2;
                        continue;
                    }

                    inQuotes = false;
                    i++;
                    continue;
                }

                field.Append(c);
                i++;
                continue;
            }

            switch (c)
            {
                case '"':
                    inQuotes = true;
                    i++;
                    continue;
                case ',':
                    current.Add(field.ToString());
                    field.Clear();
                    i++;
                    continue;
                case '\r':
                    i++;
                    continue;
                case '\n':
                    current.Add(field.ToString());
                    field.Clear();
                    rows.Add(current);
                    current = new List<string>();
                    i++;
                    continue;
                default:
                    field.Append(c);
                    i++;
                    continue;
            }
        }

        current.Add(field.ToString());
        // Ignore a phantom trailing row produced by a final newline.
        if (current.Count > 1 || current[0].Length > 0)
        {
            rows.Add(current);
        }

        return rows;
    }
}
