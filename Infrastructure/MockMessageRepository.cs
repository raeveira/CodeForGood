using Application;
using Application.Messages;
using Application.Requirements;

namespace Infrastructure;

public class MockMessageRepository : IMessageRepository
{
    private static readonly List<Message> messages = InitializeMockMessages();

    private static List<Message> InitializeMockMessages()
    {
        List<Message> mockMessages = new();
        IRequirementRepository requirementRepository = new MockRequirementRepository();
        IReadOnlyList<Requirement> requirements = requirementRepository.GetAll();

        Random random = new Random();

        string[] senderNames = { "Developer", "Sales" };

        string[] messageTexts =
        {
            "Hello, I have a question about this requirement.",
            "Can you provide more details about the timeline?",
            "Sure, the timeline is expected to be 3 months.",
            "The customer wants this to integrate with their existing API.",
            "I need clarification on the acceptance criteria.",
            "This requirement is ready for implementation.",
            "We should schedule a meeting to discuss this.",
            "I've found a potential issue with the current approach.",
            "Let's prioritize this for the next sprint.",
            "The design mockups are ready for review.",
            "I've completed the initial implementation.",
            "We need to test this more thoroughly before release.",
            "The customer has approved this requirement.",
            "I recommend we break this down into smaller tasks.",
            "This feature is now in production.",
            "Can we revisit this requirement after the first release?",
            "The performance metrics are better than expected.",
            "We should document this for future reference."
        };

        for (int index = 0; index < 25; index++)
        {
            Requirement randomRequirement = requirements[random.Next(requirements.Count)];
            string randomSender = senderNames[random.Next(senderNames.Length)];
            string randomMessage = messageTexts[random.Next(messageTexts.Length)];

            Message message = Message.Create(randomRequirement.Id, randomSender, randomMessage);
            mockMessages.Add(message);
        }

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
