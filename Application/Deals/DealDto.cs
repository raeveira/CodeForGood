namespace Application.Deals;

public sealed record DealDto(
    int Id,
    string Title,
    string Customer,
    string Status,
    string Description,
    DateOnly EndDate
);
