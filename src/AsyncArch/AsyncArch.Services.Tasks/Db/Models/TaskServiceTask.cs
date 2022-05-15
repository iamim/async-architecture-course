using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AsyncArch.Services.Tasks.Db.Models;

[Index(nameof(Assignee), IsUnique = false)]
public class TaskServiceTask
{
    public int Id { get; set; }
    
    public Guid Uuid { get; set; }
    
    public Guid Assignee { get; set; }

    [Required]
    public string Description { get; set; } = null!;
    
    public bool IsDone { get; set; }
}