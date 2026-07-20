using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace WordUp.Infrastructure;

/// <summary>
/// EF models contain value-converter delegates created from an <see cref="AppDbContext"/> instance.
/// Include the non-secret protection-key identifier in the cache key so a model whose delegates
/// captured one protector can never be reused by a context configured with another key.
/// </summary>
public sealed class SensitiveDataModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime) => context is AppDbContext appDbContext
        ? (context.GetType(), appDbContext.SensitiveDataKeyIdentifier, designTime)
        : (context.GetType(), designTime);
}
