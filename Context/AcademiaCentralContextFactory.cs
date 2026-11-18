using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AcademiaCentralApi.Context;

public class AcademiaCentralContextFactory : IDesignTimeDbContextFactory<AcademiaCentralContext>
{
    public AcademiaCentralContext CreateDbContext(string[] args)
    {
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddUserSecrets<AcademiaCentralContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta configurar ConnectionStrings:DefaultConnection.");

        DbContextOptions<AcademiaCentralContext> options = new DbContextOptionsBuilder<AcademiaCentralContext>()
            .UseSqlite(connectionString)
            .Options;

        return new AcademiaCentralContext(options);
    }
}
