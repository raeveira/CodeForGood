using Application.Deals;

namespace Presentation.Models;

public sealed class DealSortOption
{
    private readonly Func<IEnumerable<DealResponseDto>, IEnumerable<DealResponseDto>> apply;

    private DealSortOption(string group, string key, string label, Func<IEnumerable<DealResponseDto>, IEnumerable<DealResponseDto>> apply)
    {
        Group = group;
        Key = key;
        Label = label;
        this.apply = apply;
    }

    public string Group { get; }
    public string Key { get; }
    public string Label { get; }

    public IReadOnlyList<DealResponseDto> Apply(IEnumerable<DealResponseDto> deals) => [.. apply(deals)];

    // Status options are generated from the DealStatus enum, so new statuses appear automatically.
    public static IReadOnlyList<DealSortOption> GetAll()
    {
        List<DealSortOption> options =
        [
            new("General", "default", "Default", deals => deals
                .OrderByDescending(deal => deal.CreatedAt)
                .OrderBy(deal => StatusRank(deal.Status))),
            new("Created date", "created-desc", "Newest first", deals => deals.OrderByDescending(deal => deal.CreatedAt)),
            new("Created date", "created-asc", "Oldest first", deals => deals.OrderBy(deal => deal.CreatedAt)),
            new("Estimated Deadline", "end-asc", "Soonest first", deals => deals.OrderBy(deal => deal.EstimatedDeadline)),
            new("Estimated Deadline", "end-desc", "Latest first", deals => deals.OrderByDescending(deal => deal.EstimatedDeadline)),
            new("Title", "title", "A-Z", deals => deals.OrderBy(deal => deal.Title, StringComparer.OrdinalIgnoreCase))
        ];

        foreach (EDealStatus status in Enum.GetValues<EDealStatus>())
        {
            string name = status.ToString();
            options.Add(new("Show status first", $"first-{name}", Humanize(name), deals => deals
                .OrderByDescending(deal => deal.CreatedAt)
                .OrderBy(deal => Normalize(deal.Status) == name ? 0 : 1)));
        }

        return options;
    }

    private static string Normalize(string status) => status.Replace(" ", string.Empty, StringComparison.Ordinal);

    private static string Humanize(string name) =>
        System.Text.RegularExpressions.Regex.Replace(name, "(?<=[a-z])(?=[A-Z])", " ");

    private static int StatusRank(string status)
    {
        int index = Array.FindIndex(Enum.GetNames<EDealStatus>(), name => name == Normalize(status));
        return index < 0 ? int.MaxValue : index;
    }
}
