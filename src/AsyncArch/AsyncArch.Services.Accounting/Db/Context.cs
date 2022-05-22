using AsyncArch.Services.Accounting.Db.Models;
using Microsoft.EntityFrameworkCore;
using Task = AsyncArch.Services.Accounting.Db.Models.Task;

namespace AsyncArch.Services.Accounting.Db;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options) { }
    
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Task> Tasks { get; set; } = null!;
    public DbSet<AccountBalanceTransaction> AccountBalanceTransactions { get; set; } = null!;
    public DbSet<DailyBillingCycle> DailyBillingCycles { get; set; } = null!;
}