namespace Application.Messages;

public sealed record MessageResponseDto(
    Guid Id,
    Guid RequirementId,
    string Author,
    string Text,
    DateTime SentAt
);

