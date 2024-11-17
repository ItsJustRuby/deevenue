using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Deevenue.Infrastructure.Db;

// Only used implicitly, to generate migrations.
public class DesignTimeDeevenueContextFactory : IDesignTimeDbContextFactory<DeevenueContext>
{
    public DeevenueContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DeevenueContext>();
        // No need to specify a connection string here - you can (and need to) set it at
        // runtime of the migration executable.
        optionsBuilder.UseNpgsql();

        return new DeevenueContext(optionsBuilder.Options);
    }
}
