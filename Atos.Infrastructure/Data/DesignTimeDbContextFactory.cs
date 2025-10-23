using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atos.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AtosDbContext>
{
    public AtosDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AtosDbContext>();
        var databasePath = Path.Combine(Directory.GetCurrentDirectory(), "atos.db");
        optionsBuilder.UseSqlite($"Data Source={databasePath}");
        return new AtosDbContext(optionsBuilder.Options);
    }
}
