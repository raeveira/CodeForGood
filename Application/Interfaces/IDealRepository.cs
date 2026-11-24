using Application.Deals;
namespace Application;

public interface IDealRepository
{
    IReadOnlyList<Deal> GetAll();
    Deal? GetById(Guid id);
    void Add(Deal deal);
    void Update(Deal deal);
    void Remove(Deal deal);
}
