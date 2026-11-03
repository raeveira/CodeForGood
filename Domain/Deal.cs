namespace Domain;

public sealed class Deal
{
    public int Id { get; }
    public string Title { get; private set; }
    public string Customer { get; private set; }
    public string Description { get; private set; }
    public DateOnly EndDate { get; private set; }
    public DealStatus Status { get; private set; }

    private Deal(int id, string title, string customer, string description, DateOnly endDate, DealStatus status)
    {
        Id = id;
        Title = title;
        Customer = customer;
        Description = description;
        EndDate = endDate;
        Status = status;
    }

    public static Deal Create(int id, string title, string customer,     string description, DateOnly endDate)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("A deal needs a title.");
        if (string.IsNullOrWhiteSpace(customer)) throw new DomainException("A deal needs a customer.");

            return new Deal(id, title.Trim(), customer.Trim(), description?.Trim() ?? string.Empty, endDate, DealStatus.Open);
    }

    public void UpdateDetails(string title,     string description, DateOnly endDate)
    {
        if (Status == DealStatus.Completed)
            throw new DomainException("A completed deal cannot be changed.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("A deal needs a title.");

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        EndDate = endDate;
    }

    public void StartProgress()
    {
        if (Status != DealStatus.Open)
            throw new DomainException("Only an open deal can be started.");

        Status = DealStatus.InProgress;
    }

    public void Complete()
    {
        if (Status != DealStatus.InProgress)
            throw new DomainException("Only a deal in progress can be completed.");

        Status = DealStatus.Completed;
    }
}
