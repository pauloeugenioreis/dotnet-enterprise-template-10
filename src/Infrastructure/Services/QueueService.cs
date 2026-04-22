using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace ProjectTemplate.Infrastructure.Services;

/// <summary>
/// RabbitMQ queue service implementation
/// Provides message publishing with automatic retry using Polly
/// </summary>
public class QueueService : IQueueService
{
    private readonly IOptions<AppSettings> _appSettings;
    private readonly ILogger<QueueService> _logger;
    private string ConnectionString => _appSettings.Value.Infrastructure?.RabbitMQ?.ConnectionString ?? string.Empty;

    public QueueService(
        IOptions<AppSettings> appSettings,
        ILogger<QueueService> logger)
    {
        _appSettings = appSettings;
        _logger = logger;
    }

    public Task PublishAsync(string queueName, object payload, CancellationToken cancellationToken = default)
    {
        var message = JsonSerializer.Serialize(payload ?? new { });
        return PublishAsync(queueName, message, cancellationToken);
    }

    public async Task PublishAsync(string queueName, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            throw new InvalidOperationException(
                $"RabbitMQ connection string not configured. Cannot publish message to queue '{queueName}'.");
        }

        var factory = new ConnectionFactory
        {
            Uri = new Uri(ConnectionString)
        };

        // Retry policy: retry when broker is unreachable or channel is closed
        var retryPolicy = Policy
            .Handle<BrokerUnreachableException>()
            .Or<AlreadyClosedException>()
            .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception,
                        "Error connecting to RabbitMQ. Retry {RetryCount} after {Delay}s",
                        retryCount, timeSpan.TotalSeconds);
                });

        await Task.Run(() => retryPolicy.Execute(() =>
        {
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Declare queue (idempotent operation)
            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            // Publish message
            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: queueName,
                basicProperties: null,
                body: body);

            _logger.LogInformation("Message published to queue {Queue}", queueName);
        }), cancellationToken);
    }
}
