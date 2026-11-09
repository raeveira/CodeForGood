namespace Application.Requirements;

public interface IRequirementCollection
{
    IReadOnlyList<RequirementResponseDto> GetRequirementsForDeal(Guid dealId);
    RequirementResponseDto? GetRequirementById(Guid requirementId);
    void CreateRequirement(Guid dealId, string title, string description, ERequirementType type, ERequirementPriority priority, List<string> modules);
    void UpdateRequirement(Guid requirementId, string title, string description, ERequirementType type, ERequirementPriority priority, List<string> modules);
    void DeleteRequirement(Guid requirementId);
}
