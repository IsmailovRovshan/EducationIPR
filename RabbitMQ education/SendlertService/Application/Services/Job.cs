using Dal.UnitOfWork;
using Domain.RepositoryInterfaces;
using Services.Contracts;

namespace SendlertService.Services;

public class Job : IJob
{
    IOutboxMessageRepository _outboxMessageRepository;
    IMessageProducer _messageProducer;
    IUnitOfWork _unitOfWork;

    public Job(IOutboxMessageRepository outboxMessageRepository, IMessageProducer messageProducer,
        IUnitOfWork unitOfWork)
    {
        _outboxMessageRepository = outboxMessageRepository;
        _messageProducer = messageProducer;
        _unitOfWork = unitOfWork;
    }

    public async Task Execute()
    {
        var outboxMessages = await _outboxMessageRepository.GetListAsync(x => x.ProcessedOnUtc == null);

        foreach (var outboxMessage in outboxMessages)
        {
            await _messageProducer.SendMessage(outboxMessage.Payload, "exchange1", "queue1");
            outboxMessage.ProcessedOnUtc = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}