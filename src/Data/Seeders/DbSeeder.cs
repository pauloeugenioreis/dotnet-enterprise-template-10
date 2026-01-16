using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectTemplate.Data.Context;
using ProjectTemplate.Domain.Entities;

namespace ProjectTemplate.Data.Seeders;

/// <summary>
/// Database seeder - Creates initial data for development/testing
/// </summary>
public class DbSeeder
{
    private readonly ApplicationDbContext _context;

    // Lists for generating realistic fake data
    private static readonly string[] FirstNames = { "João", "Maria", "Pedro", "Ana", "Lucas", "Juliana", "Carlos", "Beatriz", "Rafael", "Camila", "Fernando", "Patricia", "Rodrigo", "Amanda", "Bruno", "Mariana", "Diego", "Larissa", "Thiago", "Gabriela" };
    private static readonly string[] LastNames = { "Silva", "Santos", "Oliveira", "Souza", "Lima", "Ferreira", "Costa", "Rodrigues", "Almeida", "Nascimento", "Araújo", "Carvalho", "Ribeiro", "Martins", "Rocha", "Monteiro", "Mendes", "Barbosa", "Freitas", "Cardoso" };
    private static readonly string[] ProductPrefixes = { "Notebook", "Mouse", "Teclado", "Monitor", "Webcam", "Headset", "SSD", "HD Externo", "Placa de Vídeo", "Memória RAM", "Processador", "Gabinete", "Fonte", "Cooler", "Mousepad", "Cadeira Gamer", "Mesa Gamer", "Suporte para Monitor", "Hub USB", "Switch" };
    private static readonly string[] ProductSuffixes = { "Pro", "Plus", "Max", "Ultra", "Premium", "Gamer", "Professional", "Advanced", "Elite", "Deluxe", "Standard", "Basic", "Essential", "RGB", "Wireless", "Mecânico", "Óptico", "4K", "Full HD", "Portátil" };
    private static readonly string[] Streets = { "Rua das Flores", "Av. Paulista", "Rua Augusta", "Av. Brasil", "Rua XV de Novembro", "Av. Atlântica", "Rua Oscar Freire", "Av. Ipiranga", "Rua da Consolação", "Av. Faria Lima" };
    private static readonly string[] Cities = { "São Paulo", "Rio de Janeiro", "Belo Horizonte", "Porto Alegre", "Curitiba", "Brasília", "Salvador", "Fortaleza", "Recife", "Manaus" };
    private static readonly string[] States = { "SP", "RJ", "MG", "RS", "PR", "DF", "BA", "CE", "PE", "AM" };

    public DbSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Seeds the database with initial data
    /// </summary>
    public async Task SeedAsync()
    {
        // Check if database already has data
        if (await _context.Products.AnyAsync())
        {
            Console.WriteLine("Database already seeded. Skipping seed.");
            return;
        }

        Console.WriteLine("Starting database seed...");

        // Seed Roles and Admin User
        await SeedRolesAndAdminUserAsync();
        Console.WriteLine("✓ Seeded roles and admin user");

        // Seed Products (150 products)
        var products = await SeedProductsAsync(150);
        Console.WriteLine($"✓ Seeded {products.Count} products");

        // Seed Orders (120 orders with items)
        var orderCount = await SeedOrdersAsync(120, products);
        Console.WriteLine($"✓ Seeded {orderCount} orders with items");

        Console.WriteLine("Database seed completed successfully!");
    }

    /// <summary>
    /// Seeds roles and creates the default admin user
    /// Default Credentials: admin / Admin@2026!Secure
    /// </summary>
    private async Task SeedRolesAndAdminUserAsync()
    {
        // Create Roles
        if (!await _context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new Role
                {
                    Name = "Admin",
                    Description = "Administrator with full system access",
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Name = "User",
                    Description = "Standard user with limited access",
                    IsSystemRole = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Role
                {
                    Name = "Manager",
                    Description = "Manager with elevated access",
                    IsSystemRole = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Roles.AddRangeAsync(roles);
            await _context.SaveChangesAsync();
        }

        // Create Default Admin User
        if (!await _context.Users.AnyAsync())
        {
            // Password: Admin@2026!Secure
            var passwordHash = HashPassword("Admin@2026!Secure");

            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@projecttemplate.com",
                PasswordHash = passwordHash,
                FirstName = "System",
                LastName = "Administrator",
                PhoneNumber = "+55 11 99999-9999",
                IsActive = true,
                EmailConfirmed = true,
                TwoFactorEnabled = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(adminUser);
            await _context.SaveChangesAsync();

            // Assign Admin role
            var adminRole = await _context.Roles.FirstAsync(r => r.Name == "Admin");
            var userRole = new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                AssignedAt = DateTime.UtcNow
            };

            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();

            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("   DEFAULT ADMIN CREDENTIALS");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("   Username: admin");
            Console.WriteLine("   Password: Admin@2026!Secure");
            Console.WriteLine("   Email:    admin@projecttemplate.com");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("   ⚠️  CHANGE THIS PASSWORD IN PRODUCTION!");
            Console.WriteLine("═══════════════════════════════════════════════════════");
        }
    }

    /// <summary>
    /// Simple password hashing using SHA256 (matches AuthService implementation)
    /// </summary>
    private string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Seeds products into the database
    /// </summary>
    private async Task<List<Product>> SeedProductsAsync(int count)
    {
        var products = new List<Product>();
        var usedNames = new HashSet<string>();

        for (int i = 0; i < count; i++)
        {
            string name;
            do
            {
                var prefix = ProductPrefixes[RandomNumberGenerator.GetInt32(ProductPrefixes.Length)];
                var suffix = ProductSuffixes[RandomNumberGenerator.GetInt32(ProductSuffixes.Length)];
                name = $"{prefix} {suffix}";
            } while (usedNames.Contains(name));

            usedNames.Add(name);

            var product = new Product
            {
                Name = name,
                Description = GenerateProductDescription(name),
                Price = GeneratePrice(),
                Stock = RandomNumberGenerator.GetInt32(0, 500),
                Category = GenerateCategory(),
                IsActive = RandomNumberGenerator.GetInt32(100) > 10, // 90% active
                CreatedAt = DateTime.UtcNow.AddDays(-RandomNumberGenerator.GetInt32(1, 365)),
                UpdatedAt = DateTime.UtcNow.AddDays(-RandomNumberGenerator.GetInt32(0, 30))
            };

            products.Add(product);
        }

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        return products;
    }

    /// <summary>
    /// Seeds orders and order items into the database
    /// </summary>
    private async Task<int> SeedOrdersAsync(int count, List<Product> products)
    {
        var activeProducts = products.Where(p => p.IsActive && p.Stock > 0).ToList();
        var orders = new List<Order>();

        for (int i = 0; i < count; i++)
        {
            var customerFirstName = FirstNames[RandomNumberGenerator.GetInt32(FirstNames.Length)];
            var customerLastName = LastNames[RandomNumberGenerator.GetInt32(LastNames.Length)];
            var customerName = $"{customerFirstName} {customerLastName}";
            var customerEmail = GenerateEmail(customerFirstName, customerLastName);

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                CustomerPhone = GeneratePhone(),
                ShippingAddress = GenerateAddress(),
                Status = GenerateOrderStatus(),
                Notes = GenerateOrderNotes(),
                CreatedAt = DateTime.UtcNow.AddDays(-RandomNumberGenerator.GetInt32(1, 180)),
                UpdatedAt = DateTime.UtcNow.AddDays(-RandomNumberGenerator.GetInt32(0, 30)),
                Items = new List<OrderItem>()
            };

            // Add 1-5 items per order
            var itemCount = RandomNumberGenerator.GetInt32(1, 6);
            var selectedProducts = activeProducts.OrderBy(x => RandomNumberGenerator.GetInt32(int.MaxValue)).Take(itemCount).ToList();

            foreach (var product in selectedProducts)
            {
                var quantity = RandomNumberGenerator.GetInt32(1, 5);
                var orderItem = new OrderItem
                {
                    Order = order,
                    ProductId = product.Id,
                    Product = product,
                    ProductName = product.Name,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    Subtotal = product.Price * quantity
                };

                order.Items.Add(orderItem);
            }

            // Calculate order totals
            order.Subtotal = order.Items.Sum(i => i.Subtotal);
            order.Tax = order.Subtotal * 0.10m; // 10% tax
            order.ShippingCost = order.Subtotal >= 100 ? 0 : 15.00m; // Free shipping over $100
            order.Total = order.Subtotal + order.Tax + order.ShippingCost;

            orders.Add(order);
        }

        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        return orders.Count;
    }

    #region Helper Methods

    private static string GenerateProductDescription(string productName)
    {
        var descriptions = new[]
        {
            $"{productName} de alta qualidade, ideal para uso profissional e doméstico.",
            $"{productName} com tecnologia avançada e design moderno.",
            $"{productName} - excelente custo-benefício e garantia estendida.",
            $"{productName} desenvolvido com materiais premium.",
            $"{productName} - líder de vendas, recomendado por especialistas."
        };
        return descriptions[RandomNumberGenerator.GetInt32(descriptions.Length)];
    }

    private static decimal GeneratePrice()
    {
        var priceRanges = new[]
        {
            (RandomNumberGenerator.GetInt32(20, 100), 0.99m),      // $20-100
            (RandomNumberGenerator.GetInt32(100, 500), 0.90m),     // $100-500
            (RandomNumberGenerator.GetInt32(500, 1000), 0.95m),    // $500-1000
            (RandomNumberGenerator.GetInt32(1000, 5000), 0.00m)    // $1000-5000
        };

        var range = priceRanges[RandomNumberGenerator.GetInt32(priceRanges.Length)];
        return range.Item1 + range.Item2;
    }

    private static string GenerateCategory()
    {
        var categories = new[] { "Eletrônicos", "Informática", "Periféricos", "Hardware", "Acessórios", "Móveis", "Games" };
        return categories[RandomNumberGenerator.GetInt32(categories.Length)];
    }

    private static string GenerateOrderNumber()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var guid = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        return $"ORD-{date}-{guid}";
    }

    private static string GenerateEmail(string firstName, string lastName)
    {
        var domains = new[] { "gmail.com", "hotmail.com", "outlook.com", "yahoo.com.br", "empresa.com.br" };
        var domain = domains[RandomNumberGenerator.GetInt32(domains.Length)];
        var emailPrefix = $"{firstName.ToLower()}.{lastName.ToLower()}".Replace(" ", "");
        return $"{emailPrefix}@{domain}";
    }

    private static string GeneratePhone()
    {
        var ddd = RandomNumberGenerator.GetInt32(11, 99);
        var prefix = RandomNumberGenerator.GetInt32(90000, 99999);
        var suffix = RandomNumberGenerator.GetInt32(1000, 9999);
        return $"({ddd}) 9{prefix}-{suffix}";
    }

    private static string GenerateAddress()
    {
        var street = Streets[RandomNumberGenerator.GetInt32(Streets.Length)];
        var number = RandomNumberGenerator.GetInt32(10, 9999);
        var city = Cities[RandomNumberGenerator.GetInt32(Cities.Length)];
        var state = States[RandomNumberGenerator.GetInt32(States.Length)];
        var zipCode = $"{RandomNumberGenerator.GetInt32(10000, 99999)}-{RandomNumberGenerator.GetInt32(100, 999)}";

        return $"{street}, {number} - {city}/{state} - CEP: {zipCode}";
    }

    private static string GenerateOrderStatus()
    {
        var statusWeights = new[]
        {
            ("Pending", 10),
            ("Processing", 15),
            ("Shipped", 25),
            ("Delivered", 45),
            ("Cancelled", 5)
        };

        var totalWeight = statusWeights.Sum(x => x.Item2);
        var randomValue = RandomNumberGenerator.GetInt32(totalWeight);
        var cumulative = 0;

        foreach (var (status, weight) in statusWeights)
        {
            cumulative += weight;
            if (randomValue < cumulative)
            {
                return status;
            }
        }

        return "Pending";
    }

    private static string? GenerateOrderNotes()
    {
        if (RandomNumberGenerator.GetInt32(100) > 70) // 30% of orders have notes
        {
            var notes = new[]
            {
                "Cliente solicitou entrega rápida",
                "Pedido urgente",
                "Deixar com portaria",
                "Cliente VIP",
                "Primeira compra - oferecer desconto na próxima",
                "Cliente preferencial"
            };
            return notes[RandomNumberGenerator.GetInt32(notes.Length)];
        }
        return null;
    }

    #endregion
}
