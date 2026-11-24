namespace Application.Messages;

public sealed class Message
{
    public Guid Id { get; }
    public Guid RequirementId { get; }
    public string Author { get; private set; }
    public string Text { get; private set; }
    public DateTime SentAt { get; private set; }
    public DateOnly? RequestedDeadline { get; private set; }
    public bool DeadlineAdapted { get; private set; }

    private Message(Guid id, Guid requirementId, string author, string text, DateTime sentAt)
    {
        Id = id;
        RequirementId = requirementId;
        Author = author;
        Text = text;
        SentAt = sentAt;
    }

    public static Message Create(Guid requirementId, string author, string text)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("A message needs an author.");
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("A message needs text content.");

        Guid generatedId = Guid.NewGuid();
        DateTime currentTime = DateTime.UtcNow;

        return new Message(
            generatedId,
            requirementId,
            author.Trim(),
            text.Trim(),
            currentTime
        );
    }

    public void UpdateText(string newText)
    {
        if (string.IsNullOrWhiteSpace(newText))
            throw new ArgumentException("A message needs text content.");

        Text = newText.Trim();
    }

    public static Message CreateDeadlineRequest(Guid requirementId, string author, DateOnly deadline)
    {
        Message message = Create(requirementId, author, $"Requested deadline: {deadline:yyyy-MM-dd}");
        message.RequestedDeadline = deadline;
        return message;
    }

    public void AdaptDeadline()
    {
        if (RequestedDeadline is null || DeadlineAdapted)
            throw new InvalidOperationException("The deadline request is not pending.");
        DeadlineAdapted = true;
    }
}
