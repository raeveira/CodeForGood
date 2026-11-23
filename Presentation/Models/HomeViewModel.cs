using Application.Deals;
using Application.Messages;
using Application.Modules;
using Application.Requirements;

namespace Presentation.Models;

public record HomeViewModel(
    IReadOnlyList<DealResponseDto> Deals,
    DealResponseDto? SelectedDeal,
    IReadOnlyList<RequirementResponseDto> RequirementsForDeal,
    RequirementResponseDto? SelectedRequirement,
    IReadOnlyList<MessageResponseDto> MessagesForRequirement,
    IReadOnlyList<DealSortOption> SortOptions,
    string ActiveSort,
    IReadOnlyDictionary<Guid, AttentionInfo> DealAttention,
    IReadOnlyDictionary<Guid, AttentionInfo> RequirementAttention,
    IReadOnlyList<Module> AvailableModules
);



