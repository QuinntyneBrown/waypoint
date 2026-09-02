using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

public sealed class HmacAuditIntegrity : IAuditIntegrity
{
    private readonly byte[] _key;

    public HmacAuditIntegrity(IOptions<AuditIntegrityOptions> options)
    {
        _key = Encoding.UTF8.GetBytes(options.Value.SigningKey);
    }

    public string ComputeHash(AuditEntry entry)
    {
        var canonical = JsonSerializer.Serialize(new
        {
            entry.SequenceNumber,
            entry.Id,
            entry.ActorUserId,
            entry.Action,
            entry.TargetType,
            entry.TargetId,
            AtTicks = entry.At.Ticks,
            entry.Source,
            entry.IsPrivate,
            entry.Details,
            entry.PreviousHash,
        });

        return Convert.ToHexString(HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(canonical)));
    }

    public bool VerifyChain(IReadOnlyCollection<AuditEntry> entries)
    {
        if (entries.Count == 0)
        {
            return true;
        }

        var expectedSequence = 1L;
        string? previousHash = null;
        foreach (var entry in entries.OrderBy(item => item.SequenceNumber))
        {
            if (entry.SequenceNumber != expectedSequence
                || !string.Equals(entry.PreviousHash, previousHash, StringComparison.Ordinal)
                || !HashesEqual(entry.IntegrityHash, ComputeHash(entry)))
            {
                return false;
            }

            previousHash = entry.IntegrityHash;
            expectedSequence++;
        }

        return true;
    }

    private static bool HashesEqual(string? actual, string expected)
    {
        if (actual is null || actual.Length != expected.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(actual),
            Encoding.ASCII.GetBytes(expected));
    }
}
