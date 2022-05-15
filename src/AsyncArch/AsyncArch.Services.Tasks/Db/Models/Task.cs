using System.ComponentModel.DataAnnotations;

namespace AsyncArch.Services.Tasks.Db.Models;

public class Task
{
    public int Id { get; set; }
    
    public Guid Uuid { get; set; }
    
    public Guid Assignee { get; set; }

    [Required]
    public string Title { get; set; } = null!;
    
    public string? JiraId { get; set; }
    
    public bool IsDone { get; set; }
}