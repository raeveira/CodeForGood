namespace Application.Deals;

public interface IDealService
{
    IReadOnlyList<DealResponseDto> GetAll();
    DealResponseDto? GetById(Guid id);
    void Create(string title, string customer, string description, DateOnly estimatedDeadline);
    void UpdateDealDetails(Guid dealId, string title, string customer, string description, DateOnly estimatedDeadline);
    void StartDealProgress(Guid dealId);
    void CompleteDeal(Guid dealId);
    void StartDevelopment(Guid dealId);
    void CompleteDevelopment(Guid dealId);
    void ApproveDeal(Guid dealId);
    void ArchiveDeal(Guid dealId);
    void DeleteDeal(Guid dealId);
}
