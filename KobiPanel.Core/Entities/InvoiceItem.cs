namespace KobiPanel.Core.Entities;

public class InvoiceItem : BaseEntity
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
}