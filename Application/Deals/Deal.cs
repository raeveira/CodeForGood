using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Application.Deals;

public sealed class Deal
{
    public Guid Id { get; }
    public string Title { get; private set; }
    public string Customer { get; private set; }
    public string Description { get; private set; }
    public DateOnly EndDate { get; private set; }
    public DealStatus Status { get; private set; }
    public DateTime CreatedAt { get; }

    private Deal(Guid id, string title, string customer, string description, DateOnly endDate, DealStatus status, DateTime createdAt)
    {
        Id = id;
        Title = title;
        Customer = customer;
        Description = description;
        EndDate = endDate;
        Status = status;
        CreatedAt = createdAt;
    }

    public static Deal Create(string title, string customer, string description, DateOnly endDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("A deal needs a title.");
        if (string.IsNullOrWhiteSpace(customer))
            throw new ArgumentException("A deal needs a customer.");
        if (endDate < DateOnly.FromDateTime(DateTime.Now))
            throw new ArgumentException("The end date cannot be in the past.");

        Guid generatedId = Guid.NewGuid();
        return new Deal(generatedId, title.Trim(), customer.Trim(), description?.Trim() ?? string.Empty, endDate, DealStatus.TalkingStage, DateTime.Now);
    }

    public void UpdateDetails(string title, string customer, string description, DateOnly endDate)
    {
        if (Status == DealStatus.Completed)
            throw new InvalidOperationException("A completed deal cannot be changed.");
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("A deal needs a title.");
        if (string.IsNullOrWhiteSpace(customer))
            throw new ArgumentException("A deal needs a customer.");

        Title = title.Trim();
        Customer = customer.Trim();
        Description = description?.Trim() ?? string.Empty;
        EndDate = endDate;
    }

    public void StartProgress()
    {
        if (Status != DealStatus.TalkingStage)
            throw new InvalidOperationException("Only a deal in the talking stage can be started.");

        Status = DealStatus.DiscussionStage;
    }

    public void Complete()
    {
        if (Status != DealStatus.DiscussionStage)
            throw new InvalidOperationException("Only a deal in the discussion stage can be completed.");

        Status = DealStatus.OpenForDevelopment;
    }

    public void StartDevelopment()
    {
        if (Status != DealStatus.OpenForDevelopment)
            throw new InvalidOperationException("Only a deal open for development can be started.");

        Status = DealStatus.InDevelopment;
    }

    public void CompleteDevelopment()
    {
        if (Status != DealStatus.InDevelopment)
            throw new InvalidOperationException("Only a deal in development can be completed.");

        Status = DealStatus.AwaitingApproval;
    }

    public void Approve()
    {
        if (Status != DealStatus.AwaitingApproval)
            throw new InvalidOperationException("Only a deal awaiting approval can be approved.");

        Status = DealStatus.Completed;
    }

    public void Archive()
    {
        if (Status != DealStatus.Completed)
            throw new InvalidOperationException("Only a completed deal can be archived.");

        Status = DealStatus.Archived;
    }
}
