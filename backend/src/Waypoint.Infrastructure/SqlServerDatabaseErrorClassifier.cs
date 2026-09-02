using Microsoft.Data.SqlClient;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

public sealed class SqlServerDatabaseErrorClassifier : IDatabaseErrorClassifier
{
    public bool IsUniqueConstraintViolation(Exception exception, string indexName)
    {
        var sqlException = FindSqlException(exception);
        return sqlException is { Number: 2601 or 2627 }
            && sqlException.Message.Contains(indexName, StringComparison.OrdinalIgnoreCase);
    }

    private static SqlException? FindSqlException(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqlException sqlException) return sqlException;
        }
        return null;
    }
}
