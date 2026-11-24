namespace Application.Messages;

public interface IMessageService
{
    IReadOnlyList<MessageResponseDto> GetMessagesForRequirement(Guid requirementId);
    MessageResponseDto? GetMessageById(Guid messageId);
    void SendMessage(Guid requirementId, string author, string text);
    void UpdateMessage(Guid messageId, string newText);
    void DeleteMessage(Guid messageId);
    void SendDeadlineRequest(Guid requirementId, string author, DateOnly deadline);
    void AdaptDeadlineRequest(Guid messageId);
}
