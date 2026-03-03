namespace KobiPanel.Core.Entities;

public class Customer : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
    public decimal TotalSpent { get; set; } = 0;

    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public string FullName => $"{FirstName} {LastName}";
}