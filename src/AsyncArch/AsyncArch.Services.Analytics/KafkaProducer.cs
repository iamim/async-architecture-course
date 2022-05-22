namespace AsyncArch.Services.Analytics;

public class KafkaProducer
{
    public const string BusinessTopic = "accounting";
    public const string DataTopic = "accounting-stream";
    
    public async Task Send(string topic, params string[] messages)
    {
        await Task.Yield();
    }
}