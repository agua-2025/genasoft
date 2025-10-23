using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atos.Infrastructure.Data
{
    // Usada APENAS em design-time (dotnet-ef) para instanciar o DbContext
    public class AtosDbContextFactory : IDesignTimeDbContextFactory<AtosDbContext>
    {
        public AtosDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AtosDbContext>()
                .UseSqlServer(
                    // Permite sobrescrever via variável de ambiente GENASOFT_SQL, senão usa LocalDB
                    Environment.GetEnvironmentVariable("GENASOFT_SQL")
                    ?? "Server=(localdb)\\MSSQLLocalDB;Database=genasoft_dev;Trusted_Connection=True;MultipleActiveResultSets=true"
                )
                .Options;

            return new AtosDbContext(options);
        }
    }
}
