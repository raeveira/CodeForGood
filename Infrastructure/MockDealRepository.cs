using Application;
using Application.Deals;

namespace Infrastructure;

public class MockDealRepository : IDealRepository
{
    private static readonly List<Deal> deals = InitializeMockDeals();

    private static List<Deal> InitializeMockDeals()
    {
        List<Deal> mockDeals = new();

        Deal webshop = Deal.Create("Webshop", "Klant A", "Nieuwe webshop met betaalintegratie.", new DateOnly(2027, 2, 1));
        mockDeals.Add(webshop);

        Deal website = Deal.Create("Website", "Klant B", "Herontwerp van de bedrijfswebsite.", new DateOnly(2027, 3, 15));
        website.StartProgress();
        mockDeals.Add(website);

        Deal donations = Deal.Create("Donatieplatform", "Klant C", "Platform voor het beheren van donaties.", new DateOnly(2026, 12, 20));
        donations.StartProgress();
        donations.Complete();
        mockDeals.Add(donations);

        return mockDeals;
    }

    public IReadOnlyList<Deal> GetAll() => deals;

    public Deal? GetById(Guid dealId) => deals.FirstOrDefault(deal => deal.Id == dealId);

    public void Add(Deal deal)
    {
        ArgumentNullException.ThrowIfNull(deal);

        deals.Add(deal);
    }

    public void Update(Deal deal)
    {
        ArgumentNullException.ThrowIfNull(deal);

        Deal? existingDeal = deals.FirstOrDefault(existingDeal => existingDeal.Id == deal.Id);
        if (existingDeal is null)
        {
            throw new InvalidOperationException($"Deal with ID {deal.Id} not found.");
        }

        int index = deals.IndexOf(existingDeal);
        deals[index] = deal;
    }

    public void Remove(Deal deal)
    {
        ArgumentNullException.ThrowIfNull(deal);

        deals.Remove(deal);
    }
}
