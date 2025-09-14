using System.Threading.Tasks;
using Xunit;

namespace CMetalsWS.Tests.TestInfrastructure
{
    public sealed class EfFixture : IAsyncLifetime
    {
        public TestDb Db = default!;
        public TestDbContextFactory Factory = default!;

        public async Task InitializeAsync()
        {
            Db = new TestDb();
            Factory = new TestDbContextFactory(Db.Options);
            await Task.CompletedTask;
        }

        public async Task DisposeAsync() => await Db.DisposeAsync();
    }
}
