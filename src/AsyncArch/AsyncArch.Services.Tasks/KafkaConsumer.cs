using System.Text.Json;
using AsyncArch.Schema;
using AsyncArch.Services.Tasks.Db;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using static AsyncArch.Schema.Events.Accounts;
using Account = AsyncArch.Services.Tasks.Db.Models.Account;
using Task = System.Threading.Tasks.Task;

namespace AsyncArch.Services.Tasks;

public class KafkaConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<KafkaConsumer> _logger;
    public KafkaConsumer(IServiceScopeFactory scopeFactory, ILogger<KafkaConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // TODO: consider batching (no default implementation in .NET SDK)
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        
        var config = new ConsumerConfig
        {
            BootstrapServers = "broker:29092",
            GroupId = "tasks-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();

        consumer.Subscribe(new[] {"accounts", "accounts-stream"});

        while (!stoppingToken.IsCancellationRequested)
        {
            var consumeResult = consumer.Consume(stoppingToken);
            
            _logger.LogTrace("Read event from Kafka: {@Event}", consumeResult.Message.Value);
            
            using var jdoc = JsonDocument.Parse(consumeResult.Message.Value);

            if (!jdoc.RootElement.TryGetProperty("event_name", out var je) || je.ValueKind != JsonValueKind.String)
            {
                // TODO: put to dead letter queue
                _logger.LogError("No event_name property in event: {@Event}", consumeResult.Message.Value);
                continue;
            }
            
            if (!jdoc.RootElement.TryGetProperty("event_name", out var jv) || !jv.TryGetInt32(out var eventVersion))
            {
                // TODO: put to dead letter queue
                _logger.LogError("No event_version property in event: {@Event}", consumeResult.Message.Value);
                continue;
            }
            
            var eventName = je.GetString();

            if (eventName == RoleChanged_V1.Kind && eventVersion == 1) 
            {
                var e = 
                    JsonSerializer.Deserialize<RoleChanged_V1>(consumeResult.Message.Value, Json.Options)
                    ?? throw new Exception($"failed to deserialize {nameof(RoleChanged_V1)}");
                
                using var scope = _scopeFactory.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<Context>();

                var existing =
                    await context.Accounts.FirstOrDefaultAsync(
                        _ => _.AccountGuid == e.data.public_id,
                        cancellationToken: stoppingToken
                    );
                
                if (existing == null)
                    throw new Exception($"account {e.data.public_id} not found to update role");
                
                existing.Role = e.data.role;
                await context.SaveChangesAsync(CancellationToken.None);
            }
            else if (eventName == Created_V1.Kind && eventVersion == 1)
            {
                var e = 
                    JsonSerializer.Deserialize<Created_V1>(consumeResult.Message.Value, Json.Options)
                    ?? throw new Exception($"failed to deserialize {nameof(Created_V1)}");
                
                using var scope = _scopeFactory.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<Context>();

                var existing =
                    await context.Accounts.FirstOrDefaultAsync(
                        _ => _.AccountGuid == e.data.public_id,
                        cancellationToken: stoppingToken
                    );

                if (existing != null)
                {
                    _logger.LogInformation( "account {@PublicId} already exists, skipping creation", e.data.public_id);
                    continue;
                }

                var account = new Account
                {
                    AccountGuid = e.data.public_id,
                    Name = e.data.full_name,
                    Role = e.data.position
                };
                
                await context.Accounts.AddAsync(account, cancellationToken: stoppingToken);
                await context.SaveChangesAsync(CancellationToken.None);
            }
            else if (eventName == Updated_V1.Kind && eventVersion == 1)
            {
                var e = 
                    JsonSerializer.Deserialize<Updated_V1>(consumeResult.Message.Value, Json.Options)
                    ?? throw new Exception($"failed to deserialize {nameof(Updated_V1)}");
                
                using var scope = _scopeFactory.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<Context>();

                var existing =
                    await context.Accounts.FirstOrDefaultAsync(
                        _ => _.AccountGuid == e.data.public_id,
                        cancellationToken: stoppingToken
                    );
                
                if (existing == null)
                    throw new Exception($"account {e.data.public_id} not found to update");

                existing.Name = e.data.full_name;
                existing.Role = e.data.position;
                await context.SaveChangesAsync(CancellationToken.None);
            }
            else if (eventName == Deleted_V1.Kind && eventVersion == 1)
            {
                var e = 
                    JsonSerializer.Deserialize<Deleted_V1>(consumeResult.Message.Value, Json.Options)
                    ?? throw new Exception($"failed to deserialize {nameof(Deleted_V1)}");
                
                using var scope = _scopeFactory.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<Context>();

                var existing =
                    await context.Accounts.FirstOrDefaultAsync(
                        _ => _.AccountGuid == e.data.public_id,
                        cancellationToken: stoppingToken
                    );
                
                if (existing == null)
                    throw new Exception($"account {e.data.public_id} not found to delete");
                
                context.Accounts.Remove(existing);
                await context.SaveChangesAsync(CancellationToken.None);
            }
        }

        consumer.Close();
    }
}