namespace Services.Contracts;

public interface IMessageProducer
{
    Task SendMessage(string message, string exchangeName, string routingKey);
}
