namespace Application.Messages;

internal sealed class MessageService(IMessageRepository messageRepository) : IMessageService
{
    public IReadOnlyList<MessageResponseDto> GetMessagesForRequirement(Guid requirementId)
    {
        IReadOnlyList<Message> messages = messageRepository.GetByRequirementId(requirementId);
        return messages.Select(ConvertToDto).ToList();
    }

    public MessageResponseDto? GetMessageById(Guid messageId)
    {
        Message? message = messageRepository.GetById(messageId);
        return message is null ? null : ConvertToDto(message);
    }

    public void SendMessage(Guid requirementId, string author, string text)
    {
        Message newMessage = Message.Create(requirementId, author, text);
        messageRepository.Add(newMessage);
    }

    public void SendDeadlineRequest(Guid requirementId, string author, DateOnly deadline)
    {
        messageRepository.Add(Message.CreateDeadlineRequest(requirementId, author, deadline));
    }

    public void AdaptDeadlineRequest(Guid messageId)
    {
        Message message = messageRepository.GetById(messageId)
            ?? throw new InvalidOperationException($"Message with ID {messageId} not found.");
        message.AdaptDeadline();
        messageRepository.Update(message);
    }

    public void UpdateMessage(Guid messageId, string newText)
    {
        Message? message = messageRepository.GetById(messageId);
        if (message is null)
            throw new InvalidOperationException($"Message with ID {messageId} not found.");

        message.UpdateText(newText);
        messageRepository.Update(message);
    }

    public void DeleteMessage(Guid messageId)
    {
        Message? message = messageRepository.GetById(messageId);
        if (message is null)
            throw new InvalidOperationException($"Message with ID {messageId} not found.");

        messageRepository.Remove(message);
    }

    private static MessageResponseDto ConvertToDto(Message message)
    {
        return new MessageResponseDto(
            message.Id,
            message.RequirementId,
            message.Author,
            message.Text,
            message.SentAt,
            message.RequestedDeadline,
            message.DeadlineAdapted
        );
    }
}
