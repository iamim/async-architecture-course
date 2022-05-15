namespace AsyncArch.Services.Tasks.Events;

public class TaskCreated
{
    public Guid task_uuid { get; set; }
    public Guid assignee { get; set; }
    public string title { get; set; }
    public string? description { get; set; }
    public bool done { get; set; }
}