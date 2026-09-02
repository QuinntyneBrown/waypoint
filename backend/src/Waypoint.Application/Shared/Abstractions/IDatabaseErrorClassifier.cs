namespace Waypoint.Application.Shared.Abstractions;

public interface IDatabaseErrorClassifier
{
    bool IsUniqueConstraintViolation(Exception exception, string indexName);
}
