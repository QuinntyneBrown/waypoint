using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Infrastructure;

/// <summary>
/// Commits each email-producing request's business writes and durable email envelopes together.
/// The outbox sender may save more than once inside a handler; the surrounding transaction keeps
/// every save atomic without coupling application handlers to EF Core transactions.
/// </summary>
public sealed class AtomicOutboxBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly AppDbContext _db;
    private readonly AfterCommitDispatcher _afterCommit;

    public AtomicOutboxBehavior(AppDbContext db, AfterCommitDispatcher afterCommit)
    {
        _db = db;
        _afterCommit = afterCommit;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IAtomicRequest || !_db.Database.IsRelational())
        {
            return await next();
        }
        if (_db.Database.CurrentTransaction is not null)
        {
            if (!_afterCommit.IsDeferring)
                throw new InvalidOperationException(
                    "Atomic requests inside an existing transaction require a managed post-commit scope.");
            return await next();
        }

        TResponse response;
        using (_afterCommit.Defer())
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                response = await next();
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                _afterCommit.Discard();
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }
        await _afterCommit.FlushAsync(CancellationToken.None);
        return response;
    }
}
