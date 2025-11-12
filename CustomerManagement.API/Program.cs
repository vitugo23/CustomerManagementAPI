using Microsoft.EntityFrameworkCore;
using CustomerManagement.Data;
using CustomerManagement.Models;
using CustomerManagement.Models.DTOs;
using CustomerManagement.Data.Interfaces;
using CustomerManagement.Data.Repositories;
using CustomerManagement.Data.Services.Interface;
using CustomerManagement.Data.Services.Implementation;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<CustomerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// Register repositories
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IStatisticsRepository, StatisticsRepository>();

// Register services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

// GET: Basic health check
app.MapGet("/", () => "Customer Management API is running!")
    .WithName("BasicHealthCheck")
    .WithOpenApi();

// GET: Detailed health check with database info
app.MapGet("/api/health", async (CustomerContext db) =>
{
    try
    {
        var customerCount = await db.Customers.CountAsync();
        var orderCount = await db.Orders.CountAsync();
        
        return Results.Ok(new
        {
            Status = "Healthy",
            Database = "Connected",
            Customers = customerCount,
            Orders = orderCount,
            Timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
})
.WithName("DetailedHealthCheck")
.WithOpenApi()
.WithSummary("Health check with database info")
.WithDescription("Check API and database health status with counts");

// GET: Get all customers
app.MapGet("/api/customers", async (CustomerContext db) =>
{
    var customers = await db.Customers
        .Include(c => c.Addresses)
        .Include(c => c.CustomerOrders)
        .Where(c => c.IsActive)
        .Select(c => new CustomerDto
        {
            CustomerId = c.CustomerId,
            FullName = $"{c.FirstName} {c.LastName}",
            Email = c.Email,
            Phone = c.Phone,
            CreatedDate = c.CreatedDate,
            IsActive = c.IsActive,
            CustomerType = c.CustomerType ?? "Individual",
            Addresses = c.Addresses.Select(a => new AddressDto
            {
                AddressId = a.AddressId,
                AddressType = a.AddressType,
                FullAddress = $"{a.Street}, {a.City}, {a.State} {a.ZipCode}",
                IsPrimary = a.IsPrimary
            }).ToList(),
            TotalOrders = c.CustomerOrders.Count,
            TotalSpent = 0
        })
        .ToListAsync();
        
    return Results.Ok(customers);
})
.WithName("GetAllCustomers")
.WithOpenApi()
.WithSummary("Get all active customers")
.WithDescription("Retrieve all active customers with their addresses and order counts");

// GET: Get customer by ID
app.MapGet("/api/customers/{id}", async (int id, CustomerContext db) =>
{
    var customer = await db.Customers
        .Include(c => c.Addresses)
        .Include(c => c.CustomerOrders)
        .FirstOrDefaultAsync(c => c.CustomerId == id);
        
    if (customer is null) return Results.NotFound("Customer not found");
    
    var customerDto = new CustomerDto
    {
        CustomerId = customer.CustomerId,
        FullName = $"{customer.FirstName} {customer.LastName}",
        Email = customer.Email,
        Phone = customer.Phone,
        CreatedDate = customer.CreatedDate,
        IsActive = customer.IsActive,
        CustomerType = customer.CustomerType ?? "Individual",
        Addresses = customer.Addresses.Select(a => new AddressDto
        {
            AddressId = a.AddressId,
            AddressType = a.AddressType,
            FullAddress = $"{a.Street}, {a.City}, {a.State} {a.ZipCode}",
            IsPrimary = a.IsPrimary
        }).ToList(),
        TotalOrders = customer.CustomerOrders.Count,
        TotalSpent = 0
    };
    
    return Results.Ok(customerDto);
})
.WithName("GetCustomerById")
.WithOpenApi()
.WithSummary("Get customer by ID")
.WithDescription("Retrieve a specific customer by their ID with full details");

// GET: Get inactive customers
app.MapGet("/api/customers/inactive", async (CustomerContext db) =>
{
    var customers = await db.Customers
        .Include(c => c.Addresses)
        .Include(c => c.CustomerOrders)
        .Where(c => !c.IsActive)
        .Select(c => new CustomerDto
        {
            CustomerId = c.CustomerId,
            FullName = $"{c.FirstName} {c.LastName}",
            Email = c.Email,
            Phone = c.Phone,
            CreatedDate = c.CreatedDate,
            IsActive = c.IsActive,
            CustomerType = c.CustomerType ?? "Individual",
            Addresses = c.Addresses.Select(a => new AddressDto
            {
                AddressId = a.AddressId,
                AddressType = a.AddressType,
                FullAddress = $"{a.Street}, {a.City}, {a.State} {a.ZipCode}",
                IsPrimary = a.IsPrimary
            }).ToList(),
            TotalOrders = c.CustomerOrders.Count,
            TotalSpent = 0
        })
        .ToListAsync();
        
    return Results.Ok(customers);
})
.WithName("GetInactiveCustomers")
.WithOpenApi()
.WithSummary("Get inactive customers")
.WithDescription("Retrieve all inactive (soft deleted) customers");

// POST: Create customer
app.MapPost("/api/customers", async (CreateCustomerDto customerDto, CustomerContext db) =>
{
    try
    {
        // Check for duplicate email
        var existingCustomer = await db.Customers
            .FirstOrDefaultAsync(c => c.Email == customerDto.Email);
            
        if (existingCustomer != null)
        {
            return Results.BadRequest(new 
            { 
                error = "Email already exists",
                message = $"A customer with email '{customerDto.Email}' already exists.",
                field = "email"
            });
        }
        
        var customer = new Customer
        {
            FirstName = customerDto.FirstName,
            LastName = customerDto.LastName,
            Email = customerDto.Email,
            Phone = customerDto.Phone,
            CustomerType = customerDto.CustomerType,
            Notes = customerDto.Notes,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };
        
        // Add primary address if provided
        if (customerDto.PrimaryAddress != null)
        {
            customer.Addresses.Add(new Address
            {
                AddressType = customerDto.PrimaryAddress.AddressType,
                Street = customerDto.PrimaryAddress.Street,
                City = customerDto.PrimaryAddress.City,
                State = customerDto.PrimaryAddress.State,
                ZipCode = customerDto.PrimaryAddress.ZipCode,
                Country = customerDto.PrimaryAddress.Country,
                IsPrimary = customerDto.PrimaryAddress.IsPrimary
            });
        }
        
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        
        // Return DTO instead of raw entity
        var responseDto = new CustomerDto
        {
            CustomerId = customer.CustomerId,
            FullName = $"{customer.FirstName} {customer.LastName}",
            Email = customer.Email,
            Phone = customer.Phone,
            CreatedDate = customer.CreatedDate,
            IsActive = customer.IsActive,
            CustomerType = customer.CustomerType ?? "Individual",
            Addresses = customer.Addresses.Select(a => new AddressDto
            {
                AddressId = a.AddressId,
                AddressType = a.AddressType,
                FullAddress = $"{a.Street}, {a.City}, {a.State} {a.ZipCode}",
                IsPrimary = a.IsPrimary
            }).ToList(),
            TotalOrders = 0,
            TotalSpent = 0
        };
        
        return Results.Created($"/api/customers/{customer.CustomerId}", responseDto);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new 
        { 
            error = "Validation failed",
            message = ex.Message
        });
    }
})
.WithName("CreateCustomer")
.WithOpenApi()
.WithSummary("Create new customer")
.WithDescription("Create a new customer with optional primary address");

// PUT: Update customer (full update)
app.MapPut("/api/customers/{id}", async (int id, UpdateCustomerDto customerDto, CustomerContext db) =>
{
    var customer = await db.Customers
        .Include(c => c.Addresses)
        .Include(c => c.CustomerOrders)
        .FirstOrDefaultAsync(c => c.CustomerId == id);
        
    if (customer is null) return Results.NotFound("Customer not found");
    
    // Check for duplicate email (excluding current customer)
    var existingCustomer = await db.Customers
        .FirstOrDefaultAsync(c => c.Email == customerDto.Email && c.CustomerId != id);
        
    if (existingCustomer != null)
    {
        return Results.BadRequest(new 
        { 
            error = "Email already exists",
            message = $"A customer with email '{customerDto.Email}' already exists.",
            field = "email"
        });
    }
    
    customer.FirstName = customerDto.FirstName;
    customer.LastName = customerDto.LastName;
    customer.Email = customerDto.Email;
    customer.Phone = customerDto.Phone;
    customer.IsActive = customerDto.IsActive;
    customer.CustomerType = customerDto.CustomerType;
    customer.Notes = customerDto.Notes;
    customer.LastUpdated = DateTime.UtcNow;
    
    await db.SaveChangesAsync();
    
    // Return DTO instead of raw entity
    var responseDto = new CustomerDto
    {
        CustomerId = customer.CustomerId,
        FullName = $"{customer.FirstName} {customer.LastName}",
        Email = customer.Email,
        Phone = customer.Phone,
        CreatedDate = customer.CreatedDate,
        IsActive = customer.IsActive,
        CustomerType = customer.CustomerType ?? "Individual",
        Addresses = customer.Addresses.Select(a => new AddressDto
        {
            AddressId = a.AddressId,
            AddressType = a.AddressType,
            FullAddress = $"{a.Street}, {a.City}, {a.State} {a.ZipCode}",
            IsPrimary = a.IsPrimary
        }).ToList(),
        TotalOrders = customer.CustomerOrders.Count,
        TotalSpent = 0
    };
    
    return Results.Ok(responseDto);
})
.WithName("UpdateCustomer")
.WithOpenApi()
.WithSummary("Update customer")
.WithDescription("Update an existing customer's information");

// PATCH: Partial update customer
app.MapPatch("/api/customers/{id}", async (int id, UpdateCustomerPartialDto customerDto, CustomerContext db) =>
{
    var customer = await db.Customers
        .Include(c => c.Addresses)
        .Include(c => c.CustomerOrders)
        .FirstOrDefaultAsync(c => c.CustomerId == id);
        
    if (customer is null) return Results.NotFound("Customer not found");
    
    // Check for duplicate email (excluding current customer) only if email is being updated
    if (!string.IsNullOrEmpty(customerDto.Email) && customerDto.Email != customer.Email)
    {
        var existingCustomer = await db.Customers
            .FirstOrDefaultAsync(c => c.Email == customerDto.Email && c.CustomerId != id);
            
        if (existingCustomer != null)
        {
            return Results.BadRequest(new 
            { 
                error = "Email already exists",
                message = $"A customer with email '{customerDto.Email}' already exists.",
                field = "email"
            });
        }
    }
    
    // Update only the fields that are provided
    if (!string.IsNullOrEmpty(customerDto.FirstName))
        customer.FirstName = customerDto.FirstName;
    if (!string.IsNullOrEmpty(customerDto.LastName))
        customer.LastName = customerDto.LastName;
    if (!string.IsNullOrEmpty(customerDto.Email))
        customer.Email = customerDto.Email;
    if (!string.IsNullOrEmpty(customerDto.Phone))
        customer.Phone = customerDto.Phone;
    if (customerDto.IsActive.HasValue)
        customer.IsActive = customerDto.IsActive.Value;
    if (!string.IsNullOrEmpty(customerDto.CustomerType))
        customer.CustomerType = customerDto.CustomerType;
    if (!string.IsNullOrEmpty(customerDto.Notes))
        customer.Notes = customerDto.Notes;
        
    customer.LastUpdated = DateTime.UtcNow;
    
    await db.SaveChangesAsync();
    
    // Return DTO
    var responseDto = new CustomerDto
    {
        CustomerId = customer.CustomerId,
        FullName = $"{customer.FirstName} {customer.LastName}",
        Email = customer.Email,
        Phone = customer.Phone,
        CreatedDate = customer.CreatedDate,
        IsActive = customer.IsActive,
        CustomerType = customer.CustomerType ?? "Individual",
        Addresses = customer.Addresses.Select(a => new AddressDto
        {
            AddressId = a.AddressId,
            AddressType = a.AddressType,
            FullAddress = $"{a.Street}, {a.City}, {a.State} {a.ZipCode}",
            IsPrimary = a.IsPrimary
        }).ToList(),
        TotalOrders = customer.CustomerOrders.Count,
        TotalSpent = 0
    };
    
    return Results.Ok(responseDto);
})
.WithName("PartialUpdateCustomer")
.WithOpenApi()
.WithSummary("Partially update customer")
.WithDescription("Update only the specified fields of an existing customer");

// DELETE: Delete customer (soft delete)
app.MapDelete("/api/customers/{id}", async (int id, CustomerContext db) =>
{
    var customer = await db.Customers.FindAsync(id);
    if (customer is null) 
        return Results.NotFound(new { message = "Customer not found" });

    // Soft delete
    customer.IsActive = false;
    customer.LastUpdated = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(new 
    { 
        message = "Customer successfully deactivated (soft deleted)",
        customerId = id,
        timestamp = DateTime.UtcNow
    });
})
.WithName("DeleteCustomer")
.WithOpenApi()
.WithSummary("Delete customer")
.WithDescription("Soft delete a customer (marks as inactive)");

// GET: Search customers by name or email
app.MapGet("/api/customers/search", async (string? query, CustomerContext db) =>
{
    if (string.IsNullOrEmpty(query))
        return Results.BadRequest("Search query is required");
    
    var customers = await db.Customers
        .Include(c => c.Addresses)
        .Include(c => c.CustomerOrders)
        .Where(c => c.IsActive && 
                   (c.FirstName.Contains(query) || 
                    c.LastName.Contains(query) || 
                    c.Email.Contains(query)))
        .Select(c => new CustomerDto
        {
            CustomerId = c.CustomerId,
            FullName = $"{c.FirstName} {c.LastName}",
            Email = c.Email,
            Phone = c.Phone,
            CreatedDate = c.CreatedDate,
            IsActive = c.IsActive,
            CustomerType = c.CustomerType ?? "Individual",
            TotalOrders = c.CustomerOrders.Count,
            Addresses = c.Addresses.Select(a => new AddressDto
            {
                AddressId = a.AddressId,
                AddressType = a.AddressType,
                FullAddress = $"{a.Street}, {a.City}, {a.State} {a.ZipCode}",
                IsPrimary = a.IsPrimary
            }).ToList()
        })
        .ToListAsync();
        
    return Results.Ok(customers);
})
.WithName("SearchCustomers")
.WithOpenApi()
.WithSummary("Search customers")
.WithDescription("Search for customers by name or email using partial matches");

// GET: Filter customers by type
app.MapGet("/api/customers/filter/{customerType}", async (string customerType, CustomerContext db) =>
{
    var customers = await db.Customers
        .Include(c => c.Addresses)
        .Include(c => c.CustomerOrders)
        .Where(c => c.IsActive && c.CustomerType == customerType)
        .Select(c => new CustomerDto
        {
            CustomerId = c.CustomerId,
            FullName = $"{c.FirstName} {c.LastName}",
            Email = c.Email,
            Phone = c.Phone,
            CreatedDate = c.CreatedDate,
            IsActive = c.IsActive,
            CustomerType = c.CustomerType ?? "Individual",
            TotalOrders = c.CustomerOrders.Count,
            Addresses = c.Addresses.Select(a => new AddressDto
            {
                AddressId = a.AddressId,
                AddressType = a.AddressType,
                FullAddress = $"{a.Street}, {a.City}, {a.State} {a.ZipCode}",
                IsPrimary = a.IsPrimary
            }).ToList()
        })
        .ToListAsync();
        
    return Results.Ok(customers);
})
.WithName("FilterCustomersByType")
.WithOpenApi()
.WithSummary("Filter customers by type")
.WithDescription("Get customers filtered by type: Individual, Business, or Premium");

// GET: Customer statistics dashboard
app.MapGet("/api/customers/stats", async (CustomerContext db) =>
{
    var totalRevenue = await db.Orders.SumAsync(o => o.TotalAmount);
        
    var stats = new CustomerStatsDto
    {
        TotalCustomers = await db.Customers.CountAsync(),
        ActiveCustomers = await db.Customers.CountAsync(c => c.IsActive),
        InactiveCustomers = await db.Customers.CountAsync(c => !c.IsActive),
        TotalOrders = await db.Orders.CountAsync(),
        TotalRevenue = totalRevenue,
        CustomerTypeBreakdown = await db.Customers
            .Where(c => c.IsActive)
            .GroupBy(c => c.CustomerType)
            .Select(g => new CustomerTypeStatsDto
            {
                CustomerType = g.Key ?? "Unknown",
                Count = g.Count(),
                TotalRevenue = 0
            })
            .ToListAsync()
    };
    
    return Results.Ok(stats);
})
.WithName("GetCustomerStatistics")
.WithOpenApi()
.WithSummary("Get customer statistics")
.WithDescription("Get comprehensive dashboard statistics about customers and orders");

// GET: Get customer addresses
app.MapGet("/api/customers/{customerId}/addresses", async (int customerId, CustomerContext db) =>
{
    var customer = await db.Customers.FindAsync(customerId);
    if (customer is null) return Results.NotFound("Customer not found");
    
    var addresses = await db.Addresses
        .Where(a => a.CustomerId == customerId)
        .Select(a => new AddressDto
        {
            AddressId = a.AddressId,
            AddressType = a.AddressType,
            FullAddress = $"{a.Street}, {a.City}, {a.State} {a.ZipCode}",
            IsPrimary = a.IsPrimary
        })
        .ToListAsync();
        
    return Results.Ok(addresses);
})
.WithName("GetCustomerAddresses")
.WithOpenApi()
.WithSummary("Get customer addresses")
.WithDescription("Get all addresses for a specific customer");

// POST: Add address to customer
app.MapPost("/api/customers/{customerId}/addresses", async (int customerId, CreateAddressDto addressDto, CustomerContext db) =>
{
    var customer = await db.Customers.FindAsync(customerId);
    if (customer is null) return Results.NotFound("Customer not found");
    
    var address = new Address
    {
        CustomerId = customerId,
        AddressType = addressDto.AddressType,
        Street = addressDto.Street,
        City = addressDto.City,
        State = addressDto.State,
        ZipCode = addressDto.ZipCode,
        Country = addressDto.Country,
        IsPrimary = addressDto.IsPrimary
    };
    
    // If this is set as primary, make sure no other address is primary
    if (address.IsPrimary)
    {
        var existingAddresses = await db.Addresses
            .Where(a => a.CustomerId == customerId && a.IsPrimary)
            .ToListAsync();
        
        foreach (var existing in existingAddresses)
        {
            existing.IsPrimary = false;
        }
    }
    
    db.Addresses.Add(address);
    await db.SaveChangesAsync();
    
    // Return DTO instead of raw entity
    var responseDto = new AddressDto
    {
        AddressId = address.AddressId,
        AddressType = address.AddressType,
        FullAddress = $"{address.Street}, {address.City}, {address.State} {address.ZipCode}",
        IsPrimary = address.IsPrimary
    };
    
    return Results.Created($"/api/customers/{customerId}/addresses/{address.AddressId}", responseDto);
})
.WithName("AddCustomerAddress")
.WithOpenApi()
.WithSummary("Add address to customer")
.WithDescription("Add a new address to an existing customer");

// GET: Get all orders
app.MapGet("/api/orders", async (CustomerContext db) =>
{
    var orders = await db.Orders
        .Include(o => o.CustomerOrders)
        .ThenInclude(co => co.Customer)
        .Select(o => new OrderDto
        {
            OrderId = o.OrderId,
            OrderNumber = o.OrderNumber,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            Description = o.Description,
            Customers = o.CustomerOrders.Select(co => new CustomerSummaryDto
            {
                CustomerId = co.CustomerId,
                FullName = $"{co.Customer.FirstName} {co.Customer.LastName}",
                Email = co.Customer.Email,
                Role = co.Role
            }).ToList()
        })
        .ToListAsync();
        
    return Results.Ok(orders);
})
.WithName("GetAllOrders")
.WithOpenApi()
.WithSummary("Get all orders")
.WithDescription("Get all orders with associated customer information");

// POST: Create new order
app.MapPost("/api/orders", async (CreateOrderDto orderDto, CustomerContext db) =>
{
    var order = new Order
    {
        OrderNumber = orderDto.OrderNumber,
        TotalAmount = orderDto.TotalAmount,
        Status = orderDto.Status,
        Description = orderDto.Description,
        OrderDate = DateTime.UtcNow
    };
    
    db.Orders.Add(order);
    await db.SaveChangesAsync();
    
    // Add customer relationships
    foreach (var customerId in orderDto.CustomerIds)
    {
        var customerOrder = new CustomerOrder
        {
            CustomerId = customerId,
            OrderId = order.OrderId,
            Role = "Primary",
            AssignedDate = DateTime.UtcNow
        };
        db.CustomerOrders.Add(customerOrder);
    }
    
    await db.SaveChangesAsync();
    
    // Return DTO instead of raw entity
    var responseDto = new OrderDto
    {
        OrderId = order.OrderId,
        OrderNumber = order.OrderNumber,
        OrderDate = order.OrderDate,
        TotalAmount = order.TotalAmount,
        Status = order.Status,
        Description = order.Description,
        Customers = new List<CustomerSummaryDto>()
    };
    
    return Results.Created($"/api/orders/{order.OrderId}", responseDto);
})
.WithName("CreateOrder")
.WithOpenApi()
.WithSummary("Create new order")
.WithDescription("Create a new order and associate it with customers");

// Database seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CustomerContext>();
    try
    {
        // Ensure database is created
        context.Database.EnsureCreated();
        
        // Only seed if no data exists
        SeedData.Initialize(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred seeding the database: {ex.Message}");
    }
}

app.Run();

public partial class Program { }
