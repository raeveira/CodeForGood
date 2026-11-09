using Application.Deals;

namespace Presentation.Models;

public record HomeViewModel(IReadOnlyList<DealResponseDto> Deals, DealResponseDto? Selected);
