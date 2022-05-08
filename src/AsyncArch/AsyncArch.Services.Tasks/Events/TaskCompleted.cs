namespace AsyncArch.Services.Tasks.Events;

public class TaskCompleted
{
    public Guid task_uuid { get; set; }
    public Guid assignee_uuid { get; set; }
    public string task_description { get; set; }
}