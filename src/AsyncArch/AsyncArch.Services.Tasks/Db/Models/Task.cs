using System.ComponentModel.DataAnnotations;

namespace AsyncArch.Services.Tasks.Db.Models;

public class Task
{
    public int Id { get; set; }
    
    public Guid Uuid { get; set; }
    
    public Guid Assignee { get; set; }

    [Required]
    public string Description { get; set; } = null!;
    
    public bool IsDone { get; set; }
}