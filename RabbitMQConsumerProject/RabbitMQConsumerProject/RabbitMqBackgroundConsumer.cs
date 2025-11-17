using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace RabbitMQConsumerProject;

public class RabbitMqBackgroundConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqBackgroundConsumer> _logger;
    private IModel? _channel;
    private const string QueueName = "queue1";
    private string ExchangeName = "exchange1";

    public RabbitMqBackgroundConsumer(IConnection connection, ILogger<RabbitMqBackgroundConsumer> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Начал работу");
        _channel = _connection.CreateModel();

        // объявление exchange
        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null);

        // объявление очереди
        _channel.QueueDeclare(queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // привязка очереди к exchange
        _channel.QueueBind(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: QueueName);

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Вызвался метод ExecuteAsync");

        var consumer = new EventingBasicConsumer(_channel!);

        consumer.Received += (sender, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var order = JsonSerializer.Deserialize<ProductMessage>(message);

                _logger.LogInformation("Пришло сообщение: {msg}", message);

                // успешно обработали сообщение (гарантия доставки)
                _channel!.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Не удалось десереализовать сообщение. ");
                _channel!.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                
                // не смог обработать => обратно в очередь и далее снова пытаться обработать
                _channel!.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ consumer...");
        return base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
public class ProductMessage
{
    public List<Product> _products { set; get; } = new();
}