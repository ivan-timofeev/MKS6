using System.Diagnostics;
using System.Text;
using System.Text.Json;
using MKS6_Items.Models.DataTransferObjects.CreateOrder;
using MKS6_Items.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MKS6_Items.BackgroundServices;

public sealed class ReserveItemsRequestProcessingBackgroundService : BackgroundService
{
    private readonly ILogger<ReserveItemsRequestProcessingBackgroundService> _logger;
    private readonly IReserveItemsRequestProcessor _reserveItemsRequestProcessor;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public ReserveItemsRequestProcessingBackgroundService(
        ILogger<ReserveItemsRequestProcessingBackgroundService> logger,
        IReserveItemsRequestProcessor reserveItemsRequestProcessor,
        IConfiguration configuration)
    {
        _logger = logger;
        _reserveItemsRequestProcessor = reserveItemsRequestProcessor;

        var factory = new ConnectionFactory { HostName = configuration["RabbitMqHostName"] };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: "ReserveItemsRequest",
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
            var createOrderRequest = JsonSerializer.Deserialize<ReserveItemsRequest>(json)
                ?? throw new InvalidOperationException("Json must be of type CreateOrderRequest");

            _logger.LogInformation(
                "Started processing of CreateOrderRequest. TransactionalId: {T}",
                createOrderRequest.TransactionalId);

            _reserveItemsRequestProcessor.ProcessReserveItemsRequest(createOrderRequest);

            _logger.LogInformation(
                "CreateOrderRequest processed. TransactionalId: {T}",
                createOrderRequest.TransactionalId);

            _channel.BasicAck(eventArgs.DeliveryTag, false);
        };

        _channel.BasicConsume("ReserveItemsRequest", false, consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}
