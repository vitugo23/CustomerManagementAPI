using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using CustomerManagement.Data;

namespace CustomerManagement.Tests.Utilities
{
    public static class TestDbContextFactory
    {
        public static CustomerContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<CustomerContext>()
                .UseInMemoryDatabase(dbName) // no constraints enforced
                .Options;

            return new CustomerContext(options);
        }

        public static CustomerContext CreateSqliteContext()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<CustomerContext>()
                .UseSqlite(connection) // constraints enforced
                .Options;

            var context = new CustomerContext(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
