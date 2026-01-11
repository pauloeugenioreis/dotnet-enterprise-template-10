using ProjectTemplate.Data.Context;
using ProjectTemplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectTemplate.Data.Seeders;

/// <summary>
/// Database seeder - Creates initial data for development/testing
/// </summary>
public class DbSeeder
{
    private readonly ApplicationDbContext _context;
    private static readonly Random _random = new Random();
    
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

        // Seed Products (150 products)
        var products = await SeedProductsAsync(150);
        Console.WriteLine($"✓ Seeded {products.Count} products");

        // Seed Orders (120 orders with items)
        var orderCount = await SeedOrdersAsync(120, products);
        Console.WriteLine($"✓ Seeded {orderCount} orders with items");

        Console.WriteLine("Database seed completed successfully!");
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
                var prefix = ProductPrefixes[_random.Next(ProductPrefixes.Length)];
                var suffix = ProductSuffixes[_random.Next(ProductSuffixes.Length)];
                name = $"{prefix} {suffix}";
            } while (usedNames.Contains(name));

            usedNames.Add(name);

            var product = new Product
            {
                Name = name,
                Description = GenerateProductDescription(name),
                Price = GeneratePrice(),
                Stock = _random.Next(0, 500),
                Category = GenerateCategory(),
                IsActive = _random.Next(100) > 10, // 90% active
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 365)),
                UpdatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 30))
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
            var customerFirstName = FirstNames[_random.Next(FirstNames.Length)];
            var customerLastName = LastNames[_random.Next(LastNames.Length)];
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
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 180)),
                UpdatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 30)),
                Items = new List<OrderItem>()
            };

            // Add 1-5 items per order
            var itemCount = _random.Next(1, 6);
            var selectedProducts = activeProducts.OrderBy(x => _random.Next()).Take(itemCount).ToList();

            foreach (var product in selectedProducts)
            {
                var quantity = _random.Next(1, 5);
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
        return descriptions[_random.Next(descriptions.Length)];
    }

    private static decimal GeneratePrice()
    {
        var priceRanges = new[]
        {
            (_random.Next(20, 100), 0.99m),      // $20-100
            (_random.Next(100, 500), 0.90m),     // $100-500
            (_random.Next(500, 1000), 0.95m),    // $500-1000
            (_random.Next(1000, 5000), 0.00m)    // $1000-5000
        };
        
        var range = priceRanges[_random.Next(priceRanges.Length)];
        return range.Item1 + range.Item2;
    }

    private static string GenerateCategory()
    {
        var categories = new[] { "Eletrônicos", "Informática", "Periféricos", "Hardware", "Acessórios", "Móveis", "Games" };
        return categories[_random.Next(categories.Length)];
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
        var domain = domains[_random.Next(domains.Length)];
        var emailPrefix = $"{firstName.ToLower()}.{lastName.ToLower()}".Replace(" ", "");
        return $"{emailPrefix}@{domain}";
    }

    private static string GeneratePhone()
    {
        var ddd = _random.Next(11, 99);
        var prefix = _random.Next(90000, 99999);
        var suffix = _random.Next(1000, 9999);
        return $"({ddd}) 9{prefix}-{suffix}";
    }

    private static string GenerateAddress()
    {
        var street = Streets[_random.Next(Streets.Length)];
        var number = _random.Next(10, 9999);
        var city = Cities[_random.Next(Cities.Length)];
        var state = States[_random.Next(States.Length)];
        var zipCode = $"{_random.Next(10000, 99999)}-{_random.Next(100, 999)}";
        
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
        var randomValue = _random.Next(totalWeight);
        var cumulative = 0;

        foreach (var (status, weight) in statusWeights)
        {
            cumulative += weight;
            if (randomValue < cumulative)
                return status;
        }

        return "Pending";
    }

    private static string GenerateOrderNotes()
    {
        if (_random.Next(100) > 70) // 30% of orders have notes
        {
            var notes = new[]
            {
                "Cliente solicitou entrega rápida",
                "Pedido urgente",
                "Deixar com portaria",
                "Cliente VIP",
                "Primeira compra - oferecer desconto na próxima",
                "Cliente preferencial",
                null
            };
            return notes[_random.Next(notes.Length)];
        }
        return null;
    }

    #endregion
}
