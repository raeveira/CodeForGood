using Application.Requirements;

namespace Application.Requirements;

internal sealed class RequirementCollection(IRequirementRepository requirementRepository) : IRequirementCollection
{
    public IReadOnlyList<RequirementResponseDto> GetRequirementsForDeal(Guid dealId)
    {
        IReadOnlyList<Requirement> requirements = requirementRepository.GetByDealId(dealId);
        return requirements.Select(ConvertToDto).ToList();
    }

    public RequirementResponseDto? GetRequirementById(Guid requirementId)
    {
        Requirement? requirement = requirementRepository.GetById(requirementId);
        return requirement is null ? null : ConvertToDto(requirement);
    }

    public void CreateRequirement(Guid dealId, string title, string description, ERequirementType type, ERequirementPriority priority, List<string> modules)
    {
        Requirement newRequirement = Requirement.Create(dealId, title, description, type, priority, modules);
        requirementRepository.Add(newRequirement);
    }

    public void UpdateRequirement(Guid requirementId, string title, string description, ERequirementType type, ERequirementPriority priority, List<string> modules)
    {
        Requirement? requirement = requirementRepository.GetById(requirementId);
        if (requirement is null)
            throw new InvalidOperationException($"Requirement with ID {requirementId} not found.");

        requirement.UpdateDetails(title, description, type, priority, modules);
        requirementRepository.Update(requirement);
    }

    public void DeleteRequirement(Guid requirementId)
    {
        Requirement? requirement = requirementRepository.GetById(requirementId);
        if (requirement is null)
            throw new InvalidOperationException($"Requirement with ID {requirementId} not found.");

        requirementRepository.Remove(requirement);
    }

    public void UpdateDeadline(Guid requirementId, DateOnly deadline)
    {
        Requirement requirement = requirementRepository.GetById(requirementId)
            ?? throw new InvalidOperationException($"Requirement with ID {requirementId} not found.");
        requirement.UpdateDeadline(deadline);
        requirementRepository.Update(requirement);
    }

    private static RequirementResponseDto ConvertToDto(Requirement requirement)
    {
        string typeDisplay = requirement.Type switch
        {
            ERequirementType.Functional => "Functional",
            ERequirementType.Technical => "Technical",
            ERequirementType.NonFunctional => "Non-Functional",
            _ => requirement.Type.ToString()
        };

        string priorityDisplay = requirement.Priority switch
        {
            ERequirementPriority.Low => "Low",
            ERequirementPriority.Medium => "Medium",
            ERequirementPriority.High => "High",
            _ => requirement.Priority.ToString()
        };

        return new RequirementResponseDto(
            requirement.Id,
            requirement.DealId,
            requirement.Title,
            requirement.Description,
            typeDisplay,
            priorityDisplay,
            requirement.Modules,
            requirement.Deadline
        );
    }
}
