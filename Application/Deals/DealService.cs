using Domain;

namespace Application.Deals;

internal sealed class DealService(IDealRepository deals) : IDealService
{
    public IReadOnlyList<DealDto> GetAll() => deals.GetAll().Select(ToDto).ToList();

    public DealDto? GetById(int id)
    {
        Deal? deal = deals.GetById(id);
        return deal is null ? null : ToDto(deal);
    }

    private static DealDto ToDto(Deal deal) => new(
        deal.Id,
        deal.Title,
        deal.Customer,
        deal.Status switch
        {
            DealStatus.Open => "Open",
            DealStatus.InProgress => "In Progress",
            DealStatus.Completed => "Completed",
            _ => deal.Status.ToString()
        },
        deal.Description,
        deal.EndDate);
}
