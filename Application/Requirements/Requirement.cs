namespace Application.Requirements;

public sealed class Requirement
{
    public Guid Id { get; }
    public Guid DealId { get; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public ERequirementType Type { get; private set; }
    public ERequirementPriority Priority { get; private set; }
    public IReadOnlyList<string> Modules { get; private set; }
    public DateOnly? Deadline { get; private set; }

    private Requirement(Guid id, Guid dealId, string title, string description, ERequirementType type, ERequirementPriority priority, List<string> modules)
    {
        Id = id;
        DealId = dealId;
        Title = title;
        Description = description;
        Type = type;
        Priority = priority;
        Modules = modules.AsReadOnly();
    }

    public static Requirement Create(Guid dealId, string title, string description, ERequirementType type, ERequirementPriority priority, List<string> modules)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("A requirement needs a title.");
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A requirement needs a description.");

        Guid generatedId = Guid.NewGuid();

        List<string> validModules = modules
            .Where(module => !string.IsNullOrWhiteSpace(module))
            .Select(module => module.Trim())
            .Distinct()
            .ToList();

        return new Requirement(
            generatedId,
            dealId,
            title.Trim(),
            description.Trim(),
            type,
            priority,
            validModules
        );
    }

    public void UpdateDetails(string title, string description, ERequirementType type, ERequirementPriority priority, List<string> modules)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("A requirement needs a title.");
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A requirement needs a description.");
        if (modules is null || modules.Count == 0)
            throw new ArgumentException("A requirement needs at least one module.");

        List<string> validModules = modules
            .Where(module => !string.IsNullOrWhiteSpace(module))
            .Select(module => module.Trim())
            .Distinct()
            .ToList();

        Title = title.Trim();
        Description = description.Trim();
        Type = type;
        Priority = priority;
        Modules = validModules.AsReadOnly();
    }

    public void UpdateDeadline(DateOnly deadline)
    {
        Deadline = deadline;
    }
}
