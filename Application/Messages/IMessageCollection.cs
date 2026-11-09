namespace Application.Messages;

public interface IMessageCollection
{
    IReadOnlyList<MessageDtoResponse> GetMessagesForRequirement(Guid requirementId);
    MessageDtoResponse? GetMessageById(Guid messageId);
    void SendMessage(Guid requirementId, string author, string text);
    void UpdateMessage(Guid messageId, string newText);
    void DeleteMessage(Guid messageId);
}
