namespace Application.Deals;

public interface IDealCollection
{
    IReadOnlyList<DealResponseDto> GetAll();
    DealResponseDto? GetById(Guid id);
    void Create(string title, string customer, string description, DateOnly endDate);
    void UpdateDealDetails(Guid dealId, string title, string customer, string description, DateOnly endDate);
    void StartDealProgress(Guid dealId);
    void CompleteDeal(Guid dealId);
    void DeleteDeal(Guid dealId);
}
