using AsyncArch.Services.Tasks.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace AsyncArch.Services.Tasks.Db;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options)
        : base(options) { }

    public DbSet<TaskServiceAccount> Accounts { get; set; } = null!;
    public DbSet<TaskServiceTask> Tasks { get; set; } = null!;
}