using CMetalsWS.Data;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Tests.TestInfrastructure
{
    public sealed class TestDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public TestDbContextFactory(DbContextOptions<ApplicationDbContext> options)
            => _options = options;

        public ApplicationDbContext CreateDbContext()
            => new ApplicationDbContext(_options);
    }
}
