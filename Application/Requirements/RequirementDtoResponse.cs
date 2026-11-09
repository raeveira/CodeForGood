namespace Application.Requirements;

public sealed record RequirementDtoResponse(
    Guid Id,
    Guid DealId,
    string Title,
    string Description,
    string Type,
    string Priority,
    IReadOnlyList<string> Modules
);
