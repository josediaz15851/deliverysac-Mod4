using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DeliverySac.Api.Infrastructure;

public class DeliverySacDbContextFactory : IDesignTimeDbContextFactory<DeliverySacDbContext>
{
    public DeliverySacDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.Development.local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = Environment.GetEnvironmentVariable("DELIVERYSAC_CONNECTION")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Defina la cadena de conexión en la variable DELIVERYSAC_CONNECTION o en ConnectionStrings:DefaultConnection.");

        var options = new DbContextOptionsBuilder<DeliverySacDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new DeliverySacDbContext(options);
    }
}
