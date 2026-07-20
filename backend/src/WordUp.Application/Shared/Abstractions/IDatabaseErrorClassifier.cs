namespace WordUp.Application.Shared.Abstractions;

public interface IDatabaseErrorClassifier
{
    bool IsUniqueConstraintViolation(Exception exception, string indexName);
}
