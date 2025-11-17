using RabbitMQ.Client;
using Services.Contracts;
using System.Text;
using System.Text.Json;

namespace SendlertService.Services;

public class RabbitMQProducer : IMessageProducer
{
    private readonly IConnectionFactory _factory;

    public RabbitMQProducer(IConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task SendMessage(string message, string exchangeName, string routingKey) 
    {
        using var connection = await _factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        // если такой exchange существует - ничего не меняет. 
        // если нет - создаёт новый exchange 
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null
        );

        var props = new BasicProperties();
        props.DeliveryMode = DeliveryModes.Persistent;
        props.ContentType = "application/json";

        //var messageJson = JsonSerializer.Serialize(message);
        var messageByte = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: exchangeName,     // куда отправить (в какой exchange)
            routingKey: routingKey,
            mandatory: false,           // как именно доставить ? (ключ маршрутизации)
            basicProperties: props,     // свойства сообщения (флаг устойчивости, время жизни, заголовки)
            body: messageByte           // само сообщение (массив байт)
        );
    }
}
