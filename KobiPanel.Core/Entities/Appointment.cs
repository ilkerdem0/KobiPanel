namespace KobiPanel.Core.Entities;

public class Appointment : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = "Scheduled";
    public string? Notes { get; set; }

    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;
}