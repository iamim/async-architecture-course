namespace AsyncArch.Services.Tasks.Events;

public class TaskReAssigned
{
    public Guid task_uuid { get; set; }
    public Guid was_assignee_uuid { get; set; }
    public Guid now_assignee_uuid { get; set; }
    public string task_description { get; set; }
}