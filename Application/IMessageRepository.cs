using Application.Messages;

namespace Application;

public interface IMessageRepository
{
    IReadOnlyList<Message> GetAll();
    IReadOnlyList<Message> GetByRequirementId(Guid requirementId);
    Message? GetById(Guid messageId);
    void Add(Message message);
    void Update(Message message);
    void Remove(Message message);
}
