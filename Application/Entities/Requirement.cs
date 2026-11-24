namespace Application.Requirements;

public sealed class Requirement
{
    public Guid Id { get; }
    public Guid DealId { get; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public ERequirementType Type { get; private set; }
    public ERequirementPriority Priority { get; private set; }
    public IReadOnlyList<Guid> ModuleIds { get; private set; }
    public DateOnly? Deadline { get; private set; }

    private Requirement(Guid id, Guid dealId, string title, string description, ERequirementType type, ERequirementPriority priority, List<Guid> moduleIds)
    {
        Id = id;
        DealId = dealId;
        Title = title;
        Description = description;
        Type = type;
        Priority = priority;
        ModuleIds = moduleIds.Distinct().ToList().AsReadOnly();
    }

    public static Requirement Create(Guid dealId, string title, string description, ERequirementType type, ERequirementPriority priority, List<Guid> moduleIds)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("A requirement needs a title.");
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A requirement needs a description.");

        Guid generatedId = Guid.NewGuid();

        return new Requirement(
            generatedId,
            dealId,
            title.Trim(),
            description.Trim(),
            type,
            priority,
            moduleIds
        );
    }

    public void UpdateDetails(string title, string description, ERequirementType type, ERequirementPriority priority)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("A requirement needs a title.");
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A requirement needs a description.");
        Title = title.Trim();
        Description = description.Trim();
        Type = type;
        Priority = priority;
    }

    public void UpdateModules(IEnumerable<Guid> moduleIds)
    {
        ModuleIds = moduleIds.Distinct().ToList().AsReadOnly();
    }

    public void UpdateDeadline(DateOnly deadline)
    {
        Deadline = deadline;
    }
}
