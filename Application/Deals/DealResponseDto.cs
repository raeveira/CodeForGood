namespace Application.Deals;

public sealed record DealResponseDto(
    Guid Id,
    string Title,
    string Customer,
    string Status,
    string Description,
    DateOnly EndDate
);
