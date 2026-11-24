using Application.Requirements;

namespace Application;



public interface IRequirementRepository
{
    IReadOnlyList<Requirement> GetAll();
    IReadOnlyList<Requirement> GetByDealId(Guid dealId);
    Requirement? GetById(Guid requirementId);
    void Add(Requirement requirement);
    void Update(Requirement requirement);
    void Remove(Requirement requirement);
}
