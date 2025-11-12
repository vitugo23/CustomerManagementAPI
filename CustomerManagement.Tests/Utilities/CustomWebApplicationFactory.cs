using System;
using CustomerManagement.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CustomerManagement.Tests.Utilities
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Set environment to Test so Program.cs uses InMemory database
            builder.UseEnvironment("Test");
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            // Seed the database after the host is created
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var db = services.GetRequiredService<CustomerContext>();

                try
                {
                    db.Database.EnsureCreated();
                    SeedData.Initialize(db);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding test database: {ex.Message}");
                }
            }

            return host;
        }
    }
}
