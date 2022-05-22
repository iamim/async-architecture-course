using AsyncArch.Services.Analytics.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace AsyncArch.Services.Analytics.Db;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options) { }

    public DbSet<Account> Accounts { get; set; } = null!;
}