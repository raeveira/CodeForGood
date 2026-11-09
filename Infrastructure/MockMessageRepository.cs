using Application;
using Application.Messages;

namespace Infrastructure;

public class MockMessageRepository : IMessageRepository
{
    private static readonly List<Message> messages = InitializeMockMessages();

    private static List<Message> InitializeMockMessages()
    {
        List<Message> mockMessages = new();

        Guid requirement1Id = Guid.NewGuid();
        Guid requirement2Id = Guid.NewGuid();

        Message message1 = Message.Create(requirement1Id, "Developer", "Hello, I have a question about this requirement.");
        mockMessages.Add(message1);

        Message message2 = Message.Create(requirement1Id, "Developer", "Can you provide more details about the timeline?");
        mockMessages.Add(message2);

        Message message3 = Message.Create(requirement1Id, "Sales", "Sure, the timeline is expected to be 3 months.");
        mockMessages.Add(message3);

        Message message4 = Message.Create(requirement2Id, "Sales", "The customer wants this to integrate with their existing API.");
        mockMessages.Add(message4);

        return mockMessages;
    }

    public IReadOnlyList<Message> GetAll() => messages;

    public IReadOnlyList<Message> GetByRequirementId(Guid requirementId)
    {
        return messages.Where(message => message.RequirementId == requirementId).ToList();
    }

    public Message? GetById(Guid messageId) => messages.FirstOrDefault(message => message.Id == messageId);

    public void Add(Message message)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message));

        messages.Add(message);
    }

    public void Update(Message message)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message));

        Message? existingMessage = messages.FirstOrDefault(existingMessage => existingMessage.Id == message.Id);
        if (existingMessage is null)
            throw new InvalidOperationException($"Message with ID {message.Id} not found.");

        int index = messages.IndexOf(existingMessage);
        messages[index] = message;
    }

    public void Remove(Message message)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message));

        messages.Remove(message);
    }
}
