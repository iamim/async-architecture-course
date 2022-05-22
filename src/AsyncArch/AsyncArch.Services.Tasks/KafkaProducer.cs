namespace AsyncArch.Services.Tasks;

public class KafkaProducer
{
    public const string BusinessTopic = "tasks";
    public const string DataTopic = "tasks-stream";

    public async Task Send(string topic, params string[] messages)
    {
        await Task.Yield();
    }
}