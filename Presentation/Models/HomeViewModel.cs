using Application.Deals;

namespace Presentation.Models;

public record HomeViewModel(IReadOnlyList<DealDto> Deals, DealDto? Selected);
