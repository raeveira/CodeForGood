namespace Domain;

public interface IDealRepository
{
    IReadOnlyList<Deal> GetAll();
    Deal? GetById(int id);
}
