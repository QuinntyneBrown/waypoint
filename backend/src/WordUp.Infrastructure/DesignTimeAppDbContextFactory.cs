using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace WordUp.Infrastructure;

public sealed class DesignTimeAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=WordUp_DesignTime;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        var sensitiveData = new AesSensitiveDataProtector(Options.Create(new SensitiveDataOptions
        {
            EncryptionKey = "wordup-design-time-encryption-key-32-characters-minimum",
        }));

        return new AppDbContext(options, sensitiveData);
    }
}
