namespace AsyncArch.Services.Tasks;

public class Producer
{
    public const string BusinessTopic = "tasks";
    public const string DataTopic = "tasks-stream";
    
    public async Task Send(string topic, params string[] messages) { }
}