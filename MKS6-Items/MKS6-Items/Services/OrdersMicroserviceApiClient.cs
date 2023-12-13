using System.Diagnostics;
using System.Text;
using System.Text.Json;
using MKS6_Items.Models.DataTransferObjects.CreateOrder;
using RabbitMQ.Client;

namespace MKS6_Items.Services;

public interface IOrdersMicroserviceApiClient
{
    void MakeResponseForReserveItemsRequest(ReserveItemsResponse response);
}

public sealed class OrdersMicroserviceApiClient : IOrdersMicroserviceApiClient, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _model;

    public OrdersMicroserviceApiClient(IConfiguration configuration)
    {
        var factory = new ConnectionFactory { HostName = configuration["RabbitMqHostName"] };
        _connection = factory.CreateConnection();
        _model = _connection.CreateModel();

        _model.QueueDeclare(queue: "ReserveItemsResponse",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public void MakeResponseForReserveItemsRequest(ReserveItemsResponse response)
    {
        var json = JsonSerializer.Serialize(response);
        var body = Encoding.UTF8.GetBytes(json);

        _model.BasicPublish(exchange: "",
            routingKey: "ReserveItemsResponse",
            basicProperties: null,
            body: body);
    }

    public void Dispose()
    {
        _connection.Dispose();
        _model.Dispose();
    }
}
