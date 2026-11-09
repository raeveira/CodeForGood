namespace Application.Messages;

internal sealed class MessageCollection(IMessageRepository messageRepository) : IMessageCollection
{
    public IReadOnlyList<MessageDtoResponse> GetMessagesForRequirement(Guid requirementId)
    {
        IReadOnlyList<Message> messages = messageRepository.GetByRequirementId(requirementId);
        return messages.Select(ConvertToDto).ToList();
    }

    public MessageDtoResponse? GetMessageById(Guid messageId)
    {
        Message? message = messageRepository.GetById(messageId);
        return message is null ? null : ConvertToDto(message);
    }

    public void SendMessage(Guid requirementId, string author, string text)
    {
        Message newMessage = Message.Create(requirementId, author, text);
        messageRepository.Add(newMessage);
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

    private static MessageDtoResponse ConvertToDto(Message message)
    {
        return new MessageDtoResponse(
            message.Id,
            message.RequirementId,
            message.Author,
            message.Text,
            message.SentAt
        );
    }
}
