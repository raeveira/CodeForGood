namespace Application.Deals;

public interface IDealService
{
    IReadOnlyList<DealDto> GetAll();
    DealDto? GetById(int id);
}
