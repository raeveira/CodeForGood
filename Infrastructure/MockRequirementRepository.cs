using Application;
using Application.Requirements;
using Application.Deals;

namespace Infrastructure;

public class MockRequirementRepository : IRequirementRepository
{
    private static readonly List<Requirement> requirements = InitializeMockRequirements();

    private static List<Requirement> InitializeMockRequirements()
    {
        List<Requirement> mockRequirements = new();
        IDealRepository dealRepository = new MockDealRepository();
        IReadOnlyList<Deal> deals = dealRepository.GetAll();
        
        Random random = new Random();

        string[] requirementTitles = 
        {
            "Payment Integration",
            "User Authentication",
            "Database Migration",
            "API Documentation",
            "Performance Optimization",
            "Security Audit",
            "Mobile Responsiveness",
            "Search Functionality"
        };

        string[] requirementDescriptions =
        {
            "Integrate payment gateway for online transactions",
            "Implement secure user login and registration",
            "Migrate data from legacy system to new database",
            "Create comprehensive API documentation",
            "Optimize database queries and response times",
            "Conduct security vulnerability assessment",
            "Ensure application works on all devices",
            "Add full-text search capabilities"
        };

        ERequirementType[] types = { ERequirementType.Functional, ERequirementType.Technical };
        ERequirementPriority[] priorities = { ERequirementPriority.High, ERequirementPriority.Medium, ERequirementPriority.Low };

        string[] modules = { "Module 1", "Module 2", "Module 3", "Module 4", "Module 5" };

        for (int index = 0; index < 12; index++)
        {
            Guid randomDealId = deals[random.Next(deals.Count)].Id;
            ERequirementType randomType = types[random.Next(types.Length)];
            ERequirementPriority randomPriority = priorities[random.Next(priorities.Length)];
            
            int moduleCount = random.Next(1, 4);
            List<string> selectedModules = new();
            for (int moduleIndex = 0; moduleIndex < moduleCount; moduleIndex++)
            {
                string selectedModule = modules[random.Next(modules.Length)];
                if (!selectedModules.Contains(selectedModule))
                {
                    selectedModules.Add(selectedModule);
                }
            }

            Requirement requirement = Requirement.Create(
                randomDealId,
                requirementTitles[index % requirementTitles.Length],
                requirementDescriptions[index % requirementDescriptions.Length],
                randomType,
                randomPriority,
                selectedModules
            );
            mockRequirements.Add(requirement);
        }

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
