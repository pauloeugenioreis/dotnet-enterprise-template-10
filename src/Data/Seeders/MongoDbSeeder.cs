using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using ProjectTemplate.Domain.Entities;

namespace ProjectTemplate.Data.Seeders;

/// <summary>
/// MongoDB seeder - creates initial document data for development/testing.
/// Only runs when MongoDB is enabled and registered in the DI container.
/// </summary>
public static class MongoDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var database = serviceProvider.GetService<IMongoDatabase>();
        if (database is null)
        {
            return;
        }

        var collection = database.GetCollection<CustomerReview>("customerreviews");

        // Mongo container may still be starting. Retry ping a few times and skip seeding if unavailable.
        var pingSucceeded = false;
        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await database.RunCommandAsync<BsonDocument>(
                    new BsonDocument("ping", 1),
                    cancellationToken: cancellationToken);

                pingSucceeded = true;
                break;
            }
            catch (TimeoutException)
            {
                if (attempt == maxAttempts)
                {
                    Console.WriteLine("MongoDB is not ready after retries. Skipping MongoDB seed.");
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(2 * attempt), cancellationToken);
            }
            catch (MongoConnectionException ex)
            {
                if (ex is MongoAuthenticationException)
                {
                    Console.WriteLine("MongoDB authentication failed. Skipping MongoDB seed.");
                    return;
                }

                if (attempt == maxAttempts)
                {
                    Console.WriteLine("MongoDB connection unavailable after retries. Skipping MongoDB seed.");
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(2 * attempt), cancellationToken);
            }
        }

        if (!pingSucceeded)
        {
            return;
        }

        var hasDocuments = await collection.Find(_ => true)
            .Limit(1)
            .AnyAsync(cancellationToken);

        if (hasDocuments)
        {
            Console.WriteLine("MongoDB already seeded. Skipping MongoDB seed.");
            return;
        }

        Console.WriteLine("Starting MongoDB seed...");

        var now = DateTime.UtcNow;
        var documents = new List<CustomerReview>
        {
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProductName = "Notebook Ultra 16",
                CustomerName = "Joao Silva",
                CustomerEmail = "joao.silva@example.com",
                Rating = 5,
                Title = "Desempenho excelente",
                Comment = "Entrega rapida e performance muito boa para trabalho e jogos.",
                Tags = ["performance", "premium", "recomendado"],
                Metadata = new Dictionary<string, string>
                {
                    ["platform"] = "web",
                    ["locale"] = "pt-BR",
                    ["purchaseChannel"] = "marketplace"
                },
                IsVerifiedPurchase = true,
                IsApproved = true,
                CreatedAt = now.AddDays(-15)
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProductName = "Mouse Ergonomico Pro",
                CustomerName = "Maria Souza",
                CustomerEmail = "maria.souza@example.com",
                Rating = 4,
                Title = "Confortavel para uso diario",
                Comment = "Muito bom para longas horas, so achei o clique um pouco alto.",
                Tags = ["ergonomia", "escritorio", "custo-beneficio"],
                Metadata = new Dictionary<string, string>
                {
                    ["platform"] = "mobile",
                    ["appVersion"] = "2.3.1",
                    ["device"] = "android"
                },
                IsVerifiedPurchase = true,
                IsApproved = true,
                CreatedAt = now.AddDays(-12)
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProductName = "Monitor 4K 32",
                CustomerName = "Carlos Pereira",
                CustomerEmail = "carlos.pereira@example.com",
                Rating = 5,
                Title = "Imagem impecavel",
                Comment = "Excelente qualidade de imagem e boa calibracao de fabrica.",
                Tags = ["4k", "qualidade-imagem", "home-office"],
                Metadata = new Dictionary<string, string>
                {
                    ["platform"] = "web",
                    ["browser"] = "edge",
                    ["utmCampaign"] = "blackfriday-2026"
                },
                IsVerifiedPurchase = true,
                IsApproved = true,
                CreatedAt = now.AddDays(-10)
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProductName = "Headset Wireless X",
                CustomerName = "Ana Lima",
                CustomerEmail = "ana.lima@example.com",
                Rating = 3,
                Title = "Bom audio, bateria mediana",
                Comment = "Qualidade sonora boa, mas esperava maior autonomia.",
                Tags = ["audio", "wireless", "bateria"],
                Metadata = new Dictionary<string, string>
                {
                    ["platform"] = "web",
                    ["referrer"] = "youtube-review",
                    ["region"] = "sudeste"
                },
                IsVerifiedPurchase = false,
                IsApproved = true,
                CreatedAt = now.AddDays(-8)
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProductName = "Teclado Mecanico RGB",
                CustomerName = "Rafael Gomes",
                CustomerEmail = "rafael.gomes@example.com",
                Rating = 4,
                Title = "Muito bom para digitacao",
                Comment = "Switches firmes e RGB configuravel, gostei bastante.",
                Tags = ["teclado", "rgb", "mecanico"],
                Metadata = new Dictionary<string, string>
                {
                    ["platform"] = "desktop-app",
                    ["os"] = "windows",
                    ["layout"] = "abnt2"
                },
                IsVerifiedPurchase = true,
                IsApproved = true,
                CreatedAt = now.AddDays(-6)
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProductName = "SSD NVMe 1TB",
                CustomerName = "Camila Rocha",
                CustomerEmail = "camila.rocha@example.com",
                Rating = 5,
                Title = "Upgrade que valeu a pena",
                Comment = "Inicializacao do sistema ficou muito mais rapida.",
                Tags = ["ssd", "upgrade", "velocidade"],
                Metadata = new Dictionary<string, string>
                {
                    ["platform"] = "web",
                    ["segment"] = "gaming",
                    ["customerTier"] = "gold"
                },
                IsVerifiedPurchase = true,
                IsApproved = false,
                CreatedAt = now.AddDays(-4)
            },
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProductName = "Webcam Full HD",
                CustomerName = "Bruno Alves",
                CustomerEmail = "bruno.alves@example.com",
                Rating = 4,
                Title = "Otima para reunioes",
                Comment = "Imagem nitida e microfone razoavel para videoconferencias.",
                Tags = ["webcam", "home-office", "video"],
                Metadata = new Dictionary<string, string>
                {
                    ["platform"] = "web",
                    ["network"] = "wifi",
                    ["supportContacted"] = "false"
                },
                IsVerifiedPurchase = true,
                IsApproved = false,
                CreatedAt = now.AddDays(-2)
            }
        };

        await collection.InsertManyAsync(documents, cancellationToken: cancellationToken);

        Console.WriteLine($"MongoDB seed completed successfully! Inserted {documents.Count} customer reviews.");
    }
}
