
namespace Application.Deals;

internal sealed class DealCollection(IDealRepository dealRepository) : IDealCollection
{
    private readonly List<Deal> deals = dealRepository.GetAll().ToList();

    public IReadOnlyList<DealResponseDto> GetAll()
    {
        return deals.Select(ConvertToDto).ToList();
    }

    public DealResponseDto? GetById(Guid dealId)
    {
        Deal? deal = dealRepository.GetById(dealId);
        return deal is null ? null : ConvertToDto(deal);
    }

    public void Create(string title, string customer, string description, DateOnly endDate)
    {
        Deal newDeal = Deal.Create(title, customer, description, endDate);
        dealRepository.Add(newDeal);
    }

    public void UpdateDealDetails(Guid dealId, string title, string description, DateOnly endDate)
    {
        Deal? deal = dealRepository.GetById(dealId);
        if (deal is null)
            throw new InvalidOperationException($"Deal with ID {dealId} not found.");

        deal.UpdateDetails(title, description, endDate);
        dealRepository.Update(deal);
    }

    public void StartDealProgress(Guid dealId)
    {
        Deal? deal = dealRepository.GetById(dealId);
        if (deal is null)
            throw new InvalidOperationException($"Deal with ID {dealId} not found.");

        deal.StartProgress();
        dealRepository.Update(deal);
    }

    public void CompleteDeal(Guid dealId)
    {
        Deal? deal = dealRepository.GetById(dealId);
        if (deal is null)
            throw new InvalidOperationException($"Deal with ID {dealId} not found.");

        deal.Complete();
        dealRepository.Update(deal);
    }

    private static DealResponseDto ConvertToDto(Deal deal)
    {
        string statusDisplay = deal.Status switch
        {
            DealStatus.Open => "Open",
            DealStatus.InProgress => "In Progress",
            DealStatus.Completed => "Completed",
            _ => deal.Status.ToString()
        };

        return new DealResponseDto(
            deal.Id,
            deal.Title,
            deal.Customer,
            statusDisplay,
            deal.Description,
            deal.EndDate
        );
    }
}
