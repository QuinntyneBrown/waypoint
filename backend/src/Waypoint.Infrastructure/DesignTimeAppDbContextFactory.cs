using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace Waypoint.Infrastructure;

public sealed class DesignTimeAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=Waypoint_DesignTime;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        var sensitiveData = new AesSensitiveDataProtector(Options.Create(new SensitiveDataOptions
        {
            EncryptionKey = "waypoint-design-time-encryption-key-32-characters-minimum",
        }));

        return new AppDbContext(options, sensitiveData);
    }
}
