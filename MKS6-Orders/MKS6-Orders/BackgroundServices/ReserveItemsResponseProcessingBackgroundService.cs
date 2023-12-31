using System.Text;
using System.Text.Json;
using MKS6_Orders.Models.DataTransferObjects;
using MKS6_Orders.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MKS6_Orders.BackgroundServices;

public sealed class ReserveItemsResponseProcessingBackgroundService : BackgroundService
{
    private readonly ILogger<ReserveItemsResponseProcessingBackgroundService> _logger;
    private readonly IReserveItemsResponseProcessor _reserveItemsResponseProcessor;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public ReserveItemsResponseProcessingBackgroundService(
        ILogger<ReserveItemsResponseProcessingBackgroundService> logger,
        IReserveItemsResponseProcessor reserveItemsResponseProcessor,
        IConfiguration configuration)
    {
        _logger = logger;
        _reserveItemsResponseProcessor = reserveItemsResponseProcessor;

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMqHostName"],
            AutomaticRecoveryEnabled = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: "ReserveItemsResponse",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        
        _logger.LogInformation("Initialization was successful. Waiting for messages in the queue");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (ch, eventArgs) =>
        {
            var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            var reserveItemsResponse = JsonSerializer.Deserialize<ReserveItemsResponse>(json)
                ?? throw new InvalidOperationException("Json must be of type CreateOrderRequest");

            _logger.LogInformation(
                "Started processing of ReserveItemsResponse. TransactionalId: {T}",
                reserveItemsResponse.TransactionalId);

            _reserveItemsResponseProcessor.ProcessReserveItemsResponse(reserveItemsResponse);

            _logger.LogInformation(
                "ReserveItemsResponse processed. TransactionalId: {T}",
                reserveItemsResponse.TransactionalId);

            _channel.BasicAck(eventArgs.DeliveryTag, false);
        };

        _channel.BasicConsume("ReserveItemsResponse", false, consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}
