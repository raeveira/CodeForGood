
namespace Application.Deals;

internal sealed class DealCollection(IDealRepository dealRepository) : IDealCollection
{
    public IReadOnlyList<DealResponseDto> GetAll()
    {
        return dealRepository.GetAll().Select(ConvertToDto).ToList();
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

    public void UpdateDealDetails(Guid dealId, string title, string customer, string description, DateOnly endDate)
    {
        Deal? deal = dealRepository.GetById(dealId);
        if (deal is null)
            throw new InvalidOperationException($"Deal with ID {dealId} not found.");

        deal.UpdateDetails(title, customer, description, endDate);
        dealRepository.Update(deal);
    }

    public void DeleteDeal(Guid dealId)
    {
        Deal? deal = dealRepository.GetById(dealId);
        if (deal is null)
            throw new InvalidOperationException($"Deal with ID {dealId} not found.");

        dealRepository.Remove(deal);
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

    public void StartDevelopment(Guid dealId)
    {
        Deal? deal = dealRepository.GetById(dealId);
        if (deal is null)
            throw new InvalidOperationException($"Deal with ID {dealId} not found.");

        deal.StartDevelopment();
        dealRepository.Update(deal);
    }

    public void CompleteDevelopment(Guid dealId)
    {
        Deal? deal = dealRepository.GetById(dealId);
        if (deal is null)
            throw new InvalidOperationException($"Deal with ID {dealId} not found.");

        deal.CompleteDevelopment();
        dealRepository.Update(deal);
    }

    public void ApproveDeal(Guid dealId)
    {
        Deal? deal = dealRepository.GetById(dealId);
        if (deal is null)
            throw new InvalidOperationException($"Deal with ID {dealId} not found.");

        deal.Approve();
        dealRepository.Update(deal);
    }

    public void ArchiveDeal(Guid dealId)
    {
        Deal? deal = dealRepository.GetById(dealId);
        if (deal is null)
            throw new InvalidOperationException($"Deal with ID {dealId} not found.");

        deal.Archive();
        dealRepository.Update(deal);
    }

    private static DealResponseDto ConvertToDto(Deal deal)
    {
        string statusDisplay = deal.Status switch
        {
            DealStatus.TalkingStage => "Talking Stage",
            DealStatus.DiscussionStage => "Discussion Stage",
            DealStatus.OpenForDevelopment => "Open for Development",
            DealStatus.InDevelopment => "In Development",
            DealStatus.AwaitingApproval => "Awaiting Approval",
            DealStatus.Completed => "Completed",
            DealStatus.Archived => "Archived",
            _ => deal.Status.ToString()
        };

        return new DealResponseDto(
            Id: deal.Id,
            Title: deal.Title,
            Customer: deal.Customer,
            Status: statusDisplay,
            Description: deal.Description,
            CreatedAt: deal.CreatedAt,
            EndDate: deal.EndDate
        );
    }
}
