using Application.Deals;
using Application.Messages;
using Application.Requirements;
public record DealDetailsViewModel(
    DealResponseDto Deal,
    IReadOnlyList<RequirementResponseDto> Requirements,
    RequirementResponseDto? SelectedRequirement,
    IReadOnlyList<MessageResponseDto> Messages,
    IReadOnlyDictionary<Guid, Presentation.Models.AttentionInfo> RequirementAttention
);