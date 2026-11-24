namespace Application.Modules;

public sealed class Module
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public EIntegrationComplexity Complexity { get; private set; }

    private Module(string Name, EIntegrationComplexity Complexity)
    {
        this.Id = Guid.NewGuid();
        this.Name = Name;
        this.Complexity = Complexity;
    }

    public static Module Create(string name, EIntegrationComplexity complexity)
    {
        return new Module(name.Trim(), complexity);
    }

    public void Update(string name, EIntegrationComplexity complexity)
    {
        this.Name = name.Trim();
        this.Complexity = complexity;
    }

    public void Delete()
    {
        this.Id = Guid.Empty;
        this.Name = string.Empty;
        this.Complexity = default(EIntegrationComplexity);
    }
}
