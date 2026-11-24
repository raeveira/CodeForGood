using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Application.Deals;

public sealed class Deal
{
    public Guid Id { get; }
    public string Title { get; private set; }
    public string Customer { get; private set; }
    public string Description { get; private set; }
    public DateOnly EstimatedDeadline { get; private set; }
    public EDealStatus Status { get; private set; }
    public DateTime CreatedAt { get; }

    private Deal(string title, string customer, string description, DateOnly estimatedDeadline)
    {
        Id = Guid.NewGuid();
        Title = title;
        Customer = customer;
        Description = description;
        EstimatedDeadline = estimatedDeadline;
        Status = EDealStatus.TalkingStage;
        CreatedAt = DateTime.Now;
    }

    public static Deal Create(string title, string customer, string description, DateOnly estimatedDeadline)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("A deal needs a title.");
        if (string.IsNullOrWhiteSpace(customer))
            throw new ArgumentException("A deal needs a customer.");
        if (estimatedDeadline < DateOnly.FromDateTime(DateTime.Now))
            throw new ArgumentException("The end date cannot be in the past.");

        return new Deal(title.Trim(), customer.Trim(), description?.Trim() ?? string.Empty, estimatedDeadline);
    }

    public void UpdateDetails(string title, string customer, string description, DateOnly estimatedDeadline)
    {
        if (Status == EDealStatus.Completed)
            throw new InvalidOperationException("A completed deal cannot be changed.");
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("A deal needs a title.");
        if (string.IsNullOrWhiteSpace(customer))
            throw new ArgumentException("A deal needs a customer.");

        Title = title.Trim();
        Customer = customer.Trim();
        Description = description?.Trim() ?? string.Empty;
        EstimatedDeadline = estimatedDeadline;
    }

    public void StartProgress()
    {
        if (Status != EDealStatus.TalkingStage)
            throw new InvalidOperationException("Only a deal in the talking stage can be started.");

        Status = EDealStatus.DiscussionStage;
    }

    public void Complete()
    {
        if (Status != EDealStatus.DiscussionStage)
            throw new InvalidOperationException("Only a deal in the discussion stage can be completed.");

        Status = EDealStatus.OpenForDevelopment;
    }

    public void StartDevelopment()
    {
        if (Status != EDealStatus.OpenForDevelopment)
            throw new InvalidOperationException("Only a deal open for development can be started.");

        Status = EDealStatus.InDevelopment;
    }

    public void CompleteDevelopment()
    {
        if (Status != EDealStatus.InDevelopment)
            throw new InvalidOperationException("Only a deal in development can be completed.");

        Status = EDealStatus.AwaitingApproval;
    }

    public void Approve()
    {
        if (Status != EDealStatus.AwaitingApproval)
            throw new InvalidOperationException("Only a deal awaiting approval can be approved.");

        Status = EDealStatus.Completed;
    }

    public void Archive()
    {
        if (Status != EDealStatus.Completed)
            throw new InvalidOperationException("Only a completed deal can be archived.");

        Status = EDealStatus.Archived;
    }
}
