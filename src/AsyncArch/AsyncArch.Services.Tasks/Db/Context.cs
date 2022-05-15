using AsyncArch.Services.Tasks.Db.Models;
using Microsoft.EntityFrameworkCore;
using Task = AsyncArch.Services.Tasks.Db.Models.Task;

namespace AsyncArch.Services.Tasks.Db;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options) { }

    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Task> Tasks { get; set; } = null!;
}