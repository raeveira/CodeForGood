using Application.Modules;

namespace Application.Requirements;

public sealed record RequirementResponseDto(
    Guid Id,
    Guid DealId,
    string Title,
    string Description,
    string Type,
    string Priority,
    IReadOnlyList<Module> Modules,
    DateOnly? Deadline
);
