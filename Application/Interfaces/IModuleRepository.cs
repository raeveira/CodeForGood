using Application.Modules;

namespace Application;

public interface IModuleRepository
{
    IReadOnlyList<Module> GetAll();
    Module? GetById(Guid id);
    void Add(Module module);
    bool Update(Module module);
    bool Remove(Guid id);
}
