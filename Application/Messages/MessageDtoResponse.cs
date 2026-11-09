namespace Application.Messages;

public sealed record MessageDtoResponse(
    Guid Id,
    Guid RequirementId,
    string Author,
    string Text,
    DateTime SentAt
);
