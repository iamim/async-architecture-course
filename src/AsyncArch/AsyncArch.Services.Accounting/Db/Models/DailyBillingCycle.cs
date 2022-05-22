namespace AsyncArch.Services.Accounting.Db.Models;

public class DailyBillingCycle
{
    public enum StatusKind { Started = 1, Processed = 2 }

    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public StatusKind Status { get; set; }
}