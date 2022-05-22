namespace AsyncArch.Services.Accounting.Db.Models;

public class AccountBalanceTransaction
{
    public enum Reason { Assigned = 1, Completed = 2, Payout = 3 }

    public int Id { get; set; }
    public DateTimeOffset Time { get; set; }
    public Guid AccountGuid { get; set; }
    public Guid? RelatedToTaskGuid { get; set; }
    public Reason Explanation { get; set; }
    public int BalanceChange { get; set; }
}