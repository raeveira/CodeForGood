using Application;
using Application.Modules;

namespace Infrastructure;

public sealed class MockModuleRepository : IModuleRepository
{
    private readonly Dictionary<Guid, Module> modules = Enumerable.Range(1, 5)
        .Select(index => new Module(Guid.NewGuid(), $"Module {index}", EIntegrationComplexity.ConfigurationNeeded))
        .ToDictionary(module => module.Id);
    private readonly Lock sync = new();

    public IReadOnlyList<Module> GetAll()
    {
        lock (sync)
        {
            return modules.Values.OrderBy(module => module.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }

    public Module? GetById(Guid id)
    {
        lock (sync)
        {
            return modules.GetValueOrDefault(id);
        }
    }

    public void Add(Module module)
    {
        lock (sync)
        {
            modules.Add(module.Id, module);
        }
    }

    public bool Update(Module module)
    {
        lock (sync)
        {
            if (!modules.ContainsKey(module.Id))
                return false;

            modules[module.Id] = module;
            return true;
        }
    }

    public bool Remove(Guid id)
    {
        lock (sync)
        {
            return modules.Remove(id);
        }
    }
}
