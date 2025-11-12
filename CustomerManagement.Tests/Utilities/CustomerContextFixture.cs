using CustomerManagement.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CustomerManagement.Tests.Fixtures
{
    public class CustomerContextFixture : IDisposable
    {
        private readonly SqliteConnection _connection;
        public CustomerContext Context { get; }

        public CustomerContextFixture()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<CustomerContext>()
                .UseSqlite(_connection)
                .Options;

            Context = new CustomerContext(options);

            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }
}
