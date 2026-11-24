namespace Application.Requirements;

public interface IRequirementService
{
    IReadOnlyList<RequirementResponseDto> GetRequirementsForDeal(Guid dealId);
    RequirementResponseDto? GetRequirementById(Guid requirementId);
    void CreateRequirement(Guid dealId, string title, string description, ERequirementType type, ERequirementPriority priority);
    void UpdateRequirement(Guid requirementId, string title, string description, ERequirementType type, ERequirementPriority priority);
    void UpdateModules(Guid requirementId, IEnumerable<Guid> moduleIds);
    void DeleteRequirement(Guid requirementId);
    void UpdateDeadline(Guid requirementId, DateOnly deadline);
}
