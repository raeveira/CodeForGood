using Domain;

namespace Infrastructure;

public class MockDealRepository : IDealRepository
{
    private static readonly IReadOnlyList<Deal> Deals = CreateDeals();

    private static List<Deal> CreateDeals()
    {
        Deal webshop = Deal.Create(1, "Webshop", "Klant A", "Nieuwe webshop met betaalintegratie.",
            new DateOnly(2027, 2, 1));

        Deal website = Deal.Create(2, "Website", "Klant B", "Herontwerp van de bedrijfswebsite.",
            new DateOnly(2027, 3, 15));
        website.StartProgress();

        Deal donaties = Deal.Create(3, "Donatieplatform", "Klant C", "Platform voor het beheren van donaties.",
            new DateOnly(2026, 12, 20));
        donaties.StartProgress();
        donaties.Complete();

        return new List<Deal> { webshop, website, donaties };
    }

    public IReadOnlyList<Deal> GetAll() => Deals;

    public Deal? GetById(int id) => Deals.FirstOrDefault(d => d.Id == id);
}
