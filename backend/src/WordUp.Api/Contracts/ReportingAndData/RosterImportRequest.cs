namespace WordUp.Api.Contracts.ReportingAndData;

public record RosterImportRequest(string CsvContent, Dictionary<string, string>? ColumnMapping);
