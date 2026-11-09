namespace Application.Requirements;

public interface IRequirementCollection
{
    IReadOnlyList<RequirementDtoResponse> GetRequirementsForDeal(Guid dealId);
    RequirementDtoResponse? GetRequirementById(Guid requirementId);
    void CreateRequirement(Guid dealId, string title, string description, ERequirementType type, ERequirementPriority priority, List<string> modules);
    void UpdateRequirement(Guid requirementId, string title, string description, ERequirementType type, ERequirementPriority priority, List<string> modules);
    void DeleteRequirement(Guid requirementId);
}
