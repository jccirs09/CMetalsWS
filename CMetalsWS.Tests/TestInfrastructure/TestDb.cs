using System;
using System.Threading.Tasks;
using CMetalsWS.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Tests.TestInfrastructure
{
    public sealed class TestDb : IAsyncDisposable
    {
        private readonly SqliteConnection _conn;
        public DbContextOptions<ApplicationDbContext> Options { get; }

        public TestDb()
        {
            _conn = new SqliteConnection("DataSource=:memory:");
            _conn.Open(); // keep open for the lifetime of the fixture

            Options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_conn)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .Options;

            using var db = new ApplicationDbContext(Options);
            db.Database.EnsureCreated();
        }

        public ValueTask DisposeAsync() => _conn.DisposeAsync();
    }
}
