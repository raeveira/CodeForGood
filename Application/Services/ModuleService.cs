namespace Application.Modules;
using Application;

public class ModuleService
{
    private readonly IModuleRepository _modulesRepository;
    private readonly IRequirementRepository _requirementRepository;

    public ModuleService(IModuleRepository modulesRepository, IRequirementRepository requirementRepository)
    {
        _modulesRepository = modulesRepository;
        _requirementRepository = requirementRepository;
    }

    public IReadOnlyList<Module> GetAll()
    {
        return _modulesRepository.GetAll();
    }

    public Module GetById(Guid id)
    {
        Module? module = _modulesRepository.GetById(id);

        if (module == null)
        {
            throw new Exception("Module not found");
        }

        return module;
    }

    public void Add(string name, EIntegrationComplexity complexity)
    {
        Module module = Module.Create(name, complexity);
        _modulesRepository.Add(module);
    }

    public void Update(Guid id, string name, EIntegrationComplexity complexity)
    {
        Module? module = _modulesRepository.GetById(id);
        if (module == null)
        {
            throw new Exception("Module not found");
        }
        module.Update(name, complexity);
        _modulesRepository.Update(module);
    }
    
    public void Delete(Guid id)
    {
        Module? module = _modulesRepository.GetById(id);
        if (module == null)
        {
            throw new Exception("Module not found");
        }

        if (_requirementRepository.GetAll().Any(requirement => requirement.ModuleIds.Contains(id)))
        {
            throw new Exception("Module is in use by a requirement");
        }

        module.Delete();
        _modulesRepository.Update(module);
    }

}