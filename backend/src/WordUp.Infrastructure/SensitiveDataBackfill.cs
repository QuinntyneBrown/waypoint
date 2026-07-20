using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Infrastructure;

/// <summary>
/// Rewrites legacy plaintext rows for every AES-GCM converted property after schema migrations run.
/// Raw provider values are inspected because EF materialization intentionally makes legacy plaintext
/// readable and therefore cannot tell whether a value still needs protection.
/// </summary>
public sealed class SensitiveDataBackfill
{
    private const string ProtectedPrefix = "enc:v1:";
    private const string Probe = "word-up-sensitive-data-probe";
    private static readonly byte[] BinaryProtectedPrefix = "enc:b1:"u8.ToArray();
    private static readonly byte[] BinaryProbe = "word-up-sensitive-binary-probe"u8.ToArray();

    private readonly AppDbContext _db;
    private readonly ISensitiveDataProtector _protector;

    public SensitiveDataBackfill(AppDbContext db, ISensitiveDataProtector protector)
    {
        _db = db;
        _protector = protector;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!_db.Database.IsRelational()) return;

        var mappings = GetSensitiveMappings();
        if (mappings.Count == 0) return;

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var connection = _db.Database.GetDbConnection();
        var databaseTransaction = transaction.GetDbTransaction();
        var sql = _db.GetService<ISqlGenerationHelper>();

        try
        {
            foreach (var mapping in mappings)
            {
                var rows = await ReadRowsAsync(
                    connection,
                    databaseTransaction,
                    sql,
                    mapping,
                    cancellationToken);
                foreach (var row in rows)
                {
                    object protectedValue;
                    if (mapping.Kind == SensitiveValueKind.Text)
                    {
                        var value = (string)row.Value;
                        if (value.StartsWith(ProtectedPrefix, StringComparison.Ordinal))
                        {
                            // Authenticate existing ciphertext. A key mismatch or tampered row must
                            // stop startup instead of being silently double-encrypted as plaintext.
                            _protector.Unprotect(value);
                            continue;
                        }
                        protectedValue = _protector.Protect(value);
                    }
                    else
                    {
                        var value = (byte[])row.Value;
                        if (value.AsSpan().StartsWith(BinaryProtectedPrefix))
                        {
                            _protector.Unprotect(value);
                            continue;
                        }
                        protectedValue = _protector.Protect(value);
                    }

                    await UpdateRowAsync(
                        connection,
                        databaseTransaction,
                        sql,
                        mapping,
                        row,
                        protectedValue,
                        cancellationToken);
                }
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private List<SensitiveColumnMapping> GetSensitiveMappings()
    {
        var mappings = new List<SensitiveColumnMapping>();
        foreach (var entityType in _db.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            var primaryKey = entityType.FindPrimaryKey();
            if (tableName is null || primaryKey is null) continue;

            var storeObject = StoreObjectIdentifier.Table(tableName, entityType.GetSchema());
            var keyColumns = primaryKey.Properties
                .Select(property => property.GetColumnName(storeObject))
                .ToArray();
            if (keyColumns.Any(column => column is null)) continue;
            var mappedKeyColumns = keyColumns.Select(column => column!).ToArray();

            foreach (var property in entityType.GetProperties())
            {
                var converter = property.GetTypeMapping().Converter;
                SensitiveValueKind? kind = null;
                if (property.ClrType == typeof(string)
                    && converter?.ConvertToProvider(Probe) is string convertedProbe
                    && convertedProbe.StartsWith(ProtectedPrefix, StringComparison.Ordinal))
                {
                    kind = SensitiveValueKind.Text;
                }
                else if (property.ClrType == typeof(byte[])
                    && converter?.ConvertToProvider(BinaryProbe) is byte[] convertedBinaryProbe
                    && convertedBinaryProbe.AsSpan().StartsWith(BinaryProtectedPrefix))
                {
                    kind = SensitiveValueKind.Binary;
                }
                if (kind is null)
                {
                    continue;
                }

                var columnName = property.GetColumnName(storeObject);
                if (columnName is null) continue;
                mappings.Add(new SensitiveColumnMapping(
                    entityType.GetSchema(),
                    tableName,
                    columnName,
                    mappedKeyColumns,
                    kind.Value));
            }
        }

        return mappings
            .Distinct()
            .OrderBy(mapping => mapping.Table, StringComparer.Ordinal)
            .ThenBy(mapping => mapping.Column, StringComparer.Ordinal)
            .ToList();
    }

    private static async Task<List<SensitiveRow>> ReadRowsAsync(
        DbConnection connection,
        DbTransaction transaction,
        ISqlGenerationHelper sql,
        SensitiveColumnMapping mapping,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        var columns = mapping.KeyColumns
            .Select(sql.DelimitIdentifier)
            .Append(sql.DelimitIdentifier(mapping.Column));
        command.CommandText = $"SELECT {string.Join(", ", columns)} FROM "
            + $"{sql.DelimitIdentifier(mapping.Table, mapping.Schema)} "
            + $"WHERE {sql.DelimitIdentifier(mapping.Column)} IS NOT NULL";

        var rows = new List<SensitiveRow>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var keys = new object[mapping.KeyColumns.Length];
            for (var index = 0; index < mapping.KeyColumns.Length; index++)
            {
                keys[index] = reader.GetValue(index);
            }
            var value = reader.GetValue(mapping.KeyColumns.Length);
            rows.Add(new SensitiveRow(keys, value));
        }
        return rows;
    }

    private static async Task UpdateRowAsync(
        DbConnection connection,
        DbTransaction transaction,
        ISqlGenerationHelper sql,
        SensitiveColumnMapping mapping,
        SensitiveRow row,
        object protectedValue,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        var valueParameter = command.CreateParameter();
        valueParameter.ParameterName = "@value";
        valueParameter.Value = protectedValue;
        command.Parameters.Add(valueParameter);

        var predicates = new string[mapping.KeyColumns.Length];
        for (var index = 0; index < mapping.KeyColumns.Length; index++)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"@key{index}";
            parameter.Value = row.Keys[index];
            command.Parameters.Add(parameter);
            predicates[index] = $"{sql.DelimitIdentifier(mapping.KeyColumns[index])} = {parameter.ParameterName}";
        }

        command.CommandText = $"UPDATE {sql.DelimitIdentifier(mapping.Table, mapping.Schema)} "
            + $"SET {sql.DelimitIdentifier(mapping.Column)} = {valueParameter.ParameterName} "
            + $"WHERE {string.Join(" AND ", predicates)}";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private sealed record SensitiveColumnMapping(
        string? Schema,
        string Table,
        string Column,
        string[] KeyColumns,
        SensitiveValueKind Kind);

    private sealed record SensitiveRow(object[] Keys, object Value);

    private enum SensitiveValueKind
    {
        Text,
        Binary,
    }
}
