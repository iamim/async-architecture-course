namespace AsyncArch.Services.Accounting.Db.Models;

public class Task
{
    public int Id { get; set; }
    
    public Guid TaskGuid { get; set; }

    public string? JiraId { get; set; }
    public string? Description { get; set; }

    public uint AssignmentDeduction { get; set; }
    public uint CompletionBonus { get; set; }
}