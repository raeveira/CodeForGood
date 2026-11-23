using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Application.Deals;

public sealed record DealResponseDto(
    Guid Id,
    string Title,
    string Customer,
    string Status,
    string Description,
    DateTime CreatedAt,
    DateOnly EstimatedDeadline
);
