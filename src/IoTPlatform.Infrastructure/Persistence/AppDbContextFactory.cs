using IoTPlatform.Common.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IoTPlatform.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by <c>dotnet ef</c> for migrations. Supplies a no-op tenant
/// context (filters are irrelevant when generating schema) and reads the connection string
/// from the IOTPLATFORM_CONNECTION env var, falling back to a local default.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("IOTPLATFORM_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=iotplatform;Username=iotplatform;Password=postgres";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connection)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AppDbContext(options, new DesignTimeTenantContext());
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public Guid? CompanyId => null;
        public Guid? DepartmentId => null;
        public bool IgnoreTenantFilter => true;
    }
}
