
namespace Application.Deals;

internal sealed class DealService(IDealRepository dealRepository) : IDealService
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

    public void Create(string title, string customer, string description, DateOnly estimatedDeadline)
    {
        Deal newDeal = Deal.Create(title, customer, description, estimatedDeadline);
        dealRepository.Add(newDeal);
    }

    public void UpdateDealDetails(Guid dealId, string title, string customer, string description, DateOnly estimatedDeadline)
    {
        Deal? deal = dealRepository.GetById(dealId);
        if (deal is null)
            throw new InvalidOperationException($"Deal with ID {dealId} not found.");

        deal.UpdateDetails(title, customer, description, estimatedDeadline);
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
            EDealStatus.TalkingStage => "Talking Stage",
            EDealStatus.DiscussionStage => "Discussion Stage",
            EDealStatus.OpenForDevelopment => "Open for Development",
            EDealStatus.InDevelopment => "In Development",
            EDealStatus.AwaitingApproval => "Awaiting Approval",
            EDealStatus.Completed => "Completed",
            EDealStatus.Archived => "Archived",
            _ => deal.Status.ToString()
        };

        return new DealResponseDto(
            Id: deal.Id,
            Title: deal.Title,
            Customer: deal.Customer,
            Status: statusDisplay,
            Description: deal.Description,
            CreatedAt: deal.CreatedAt,
            EstimatedDeadline: deal.EstimatedDeadline
        );
    }
}
