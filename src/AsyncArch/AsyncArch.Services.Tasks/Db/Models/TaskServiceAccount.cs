using System.ComponentModel.DataAnnotations;

namespace AsyncArch.Services.Tasks.Db.Models;

public class TaskServiceAccount
{
    [Key]
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? Role { get; set; }
}