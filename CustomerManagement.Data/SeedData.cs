using CustomerManagement.Models;

namespace CustomerManagement.Data
{
    public static class SeedData
    {
        public static void Initialize(CustomerContext context)
        {
            // Check if database already has data - if so, don't seed
            if (context.Customers.Any())
            {
                Console.WriteLine("Database already contains data. Skipping seeding.");
                return; // DB has been seeded
            }

            Console.WriteLine("Seeding database with sample data...");

            var customers = new Customer[]
            {
                new Customer
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@email.com",
                    Phone = "(555) 123-4567",
                    CustomerType = "Individual",
                    CreatedDate = DateTime.UtcNow
                },
                new Customer
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@email.com",
                    Phone = "(555) 987-6543",
                    CustomerType = "Premium",
                    CreatedDate = DateTime.UtcNow
                },
                new Customer
                {
                    FirstName = "Acme",
                    LastName = "Corporation",
                    Email = "contact@acme.com",
                    Phone = "(555) 555-0123",
                    CustomerType = "Business",
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();

            // Add sample addresses
            var addresses = new Address[]
            {
                new Address
                {
                    CustomerId = customers[0].CustomerId,
                    AddressType = "Home",
                    Street = "123 Main St",
                    City = "Anytown",
                    State = "CA",
                    ZipCode = "12345",
                    IsPrimary = true
                },
                new Address
                {
                    CustomerId = customers[1].CustomerId,
                    AddressType = "Work",
                    Street = "456 Business Ave",
                    City = "Commerce City",
                    State = "NY",
                    ZipCode = "67890",
                    IsPrimary = true
                }
            };

            context.Addresses.AddRange(addresses);
            context.SaveChanges();

            // Add sample orders
            var orders = new Order[]
            {
                new Order
                {
                    OrderNumber = "ORD-001",
                    TotalAmount = 299.99m,
                    Status = "Completed",
                    Description = "First sample order",
                    OrderDate = DateTime.UtcNow
                },
                new Order
                {
                    OrderNumber = "ORD-002",
                    TotalAmount = 149.50m,
                    Status = "Pending",
                    Description = "Second sample order",
                    OrderDate = DateTime.UtcNow
                }
            };

            context.Orders.AddRange(orders);
            context.SaveChanges();

            // Create customer-order relationships (Many-to-Many)
            var customerOrders = new CustomerOrder[]
            {
                new CustomerOrder
                {
                    CustomerId = customers[0].CustomerId,
                    OrderId = orders[0].OrderId,
                    Role = "Primary",
                    AssignedDate = DateTime.UtcNow
                },
                new CustomerOrder
                {
                    CustomerId = customers[1].CustomerId,
                    OrderId = orders[1].OrderId,
                    Role = "Primary",
                    AssignedDate = DateTime.UtcNow
                }
            };

            context.CustomerOrders.AddRange(customerOrders);
            context.SaveChanges();

            Console.WriteLine("Database seeding completed successfully.");
        }
    }
}
