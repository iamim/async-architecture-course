using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AsyncArch.Services.Tasks.Db.Models;

[Index(nameof(Assignee), IsUnique = false)]
public class TaskServiceTask
{
    [Key]
    public Guid Uuid { get; set; }
    
    public Guid Assignee { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    public bool IsDone { get; set; }
}