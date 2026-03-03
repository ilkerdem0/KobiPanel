namespace KobiPanel.Core.Entities;

public class TransactionCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Expense";
    public string? Icon { get; set; }
    public string? Color { get; set; }

    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}