namespace KobiPanel.Core.Entities;

public class Transaction : BaseEntity
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = "Expense";
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string? PaymentMethod { get; set; }
    public bool IsScheduled { get; set; } = false; // Vadeli mi?
    public DateTime? DueDate { get; set; } // Vade tarihi
    public string PaymentStatus { get; set; } = "Completed"; // Completed, Pending, Overdue

    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;

    public int? CategoryId { get; set; }
    public TransactionCategory? Category { get; set; }

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
}