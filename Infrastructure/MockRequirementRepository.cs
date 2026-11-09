using Application;
using Application.Requirements;

namespace Infrastructure;

public class MockRequirementRepository : IRequirementRepository
{
    private static readonly List<Requirement> requirements = InitializeMockRequirements();

    private static List<Requirement> InitializeMockRequirements()
    {
        List<Requirement> mockRequirements = new();

        Guid dealId = Guid.NewGuid();

        Requirement requirement1 = Requirement.Create(
            dealId,
            "Requirement 1",
            "Description 1",
            ERequirementType.Functional,
            ERequirementPriority.High,
            new List<string> { "Module 1", "Module 2" }
        );
        mockRequirements.Add(requirement1);

        Requirement requirement2 = Requirement.Create(
            dealId,
            "Requirement 2",
            "Description 2",
            ERequirementType.Technical,
            ERequirementPriority.Medium,
            new List<string> { "Module 3" }
        );
        mockRequirements.Add(requirement2);

        Requirement requirement3 = Requirement.Create(
            dealId,
            "Requirement 3",
            "Description 3",
            ERequirementType.Functional,
            ERequirementPriority.Low,
            new List<string> { "Module 1", "Module 3" }
        );
        mockRequirements.Add(requirement3);

        return mockRequirements;
    }

    public IReadOnlyList<Requirement> GetAll() => requirements;

    public IReadOnlyList<Requirement> GetByDealId(Guid dealId)
    {
        return requirements.Where(requirement => requirement.DealId == dealId).ToList();
    }

    public Requirement? GetById(Guid requirementId) => requirements.FirstOrDefault(requirement => requirement.Id == requirementId);

    public void Add(Requirement requirement)
    {
        if (requirement is null)
            throw new ArgumentNullException(nameof(requirement));

        requirements.Add(requirement);
    }

    public void Update(Requirement requirement)
    {
        if (requirement is null)
            throw new ArgumentNullException(nameof(requirement));

        Requirement? existingRequirement = requirements.FirstOrDefault(existingRequirement => existingRequirement.Id == requirement.Id);
        if (existingRequirement is null)
            throw new InvalidOperationException($"Requirement with ID {requirement.Id} not found.");

        int index = requirements.IndexOf(existingRequirement);
        requirements[index] = requirement;
    }

    public void Remove(Requirement requirement)
    {
        if (requirement is null)
            throw new ArgumentNullException(nameof(requirement));

        requirements.Remove(requirement);
    }
}
