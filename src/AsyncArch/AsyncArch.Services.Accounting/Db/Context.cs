using AsyncArch.Services.Accounting.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace AsyncArch.Services.Accounting.Db;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options) { }
    
    public DbSet<Account> Accounts { get; set; } = null!;
}