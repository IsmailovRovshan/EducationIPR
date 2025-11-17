namespace Domain.Models;

public class OutboxMessage
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    public int Id { get; set; }     
    
    /// <summary>
    /// Когда событие произошло (когда создали объект, который положим в БД)
    /// </summary>
    public DateTime OccurredOnUtc { get; set; }  

    /// <summary>
    /// Тип события
    /// </summary>
    public string? Type { get; set; } = null!;                

    /// <summary>
    /// Сами данные JSON
    /// </summary>
    public string Payload { get; set; } = null!;

    /// <summary>
    /// Когда сообщение было успешно обработано (заполнгяется в фоновом процессе)
    /// </summary>
    public DateTime? ProcessedOnUtc { get; set; }

    /// <summary>
    /// Текст ошибки, если обработка не удалась
    /// </summary>
    public string? Error { get; set; } = null!;                 
}