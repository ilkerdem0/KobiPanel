namespace KobiPanel.Core.Entities;

public class Business : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? TaxNumber { get; set; }
    public string BusinessType { get; set; } = "General";

    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<TransactionCategory> TransactionCategories { get; set; } = new List<TransactionCategory>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}