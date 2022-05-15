using System.Text.Json;
using AsyncArch.Schema;
using AsyncArch.Schema.Events;
using AsyncArch.Services.Tasks.Db;
using AsyncArch.Services.Tasks.Db.Models;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using static System.Console;
using Task = System.Threading.Tasks.Task;

namespace AsyncArch.Services.Tasks;

public class Consumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Consumer> _logger;
    public Consumer(IServiceScopeFactory scopeFactory, ILogger<Consumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

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
            
            WriteLine("ACCOUNT");
            WriteLine(consumeResult.Message.Value);
            
            using var jdoc = JsonDocument.Parse(consumeResult.Message.Value);

            if (!jdoc.RootElement.TryGetProperty("event_name", out var je) || je.ValueKind != JsonValueKind.String)
            {
                WriteLine("no event name");
                continue;
            }
            
            var eventName = je.GetString();

            if (eventName == Account.RoleChanged_V1.Kind) 
            {
                var e = 
                    JsonSerializer.Deserialize<Account.RoleChanged_V1>(consumeResult.Message.Value, Json.Options)
                    ?? throw new Exception($"failed to deserialize {nameof(Account.RoleChanged_V1)}");
                
                WriteLine("ACCOUNT ROLE CHANGED");
                
                using var scope = _scopeFactory.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<Context>();

                var existing =
                    await context.Accounts.FirstOrDefaultAsync(
                        _ => _.UserId == e.data.public_id,
                        cancellationToken: stoppingToken
                    );
                
                if (existing == null)
                    throw new Exception($"account {e.data.public_id} not found to update role");
                
                existing.Role = e.data.role;
                await context.SaveChangesAsync(cancellationToken: stoppingToken);
            }
            else if (eventName == Account.Created_V1.Kind)
            {
                var e = 
                    JsonSerializer.Deserialize<Account.Created_V1>(consumeResult.Message.Value, Json.Options)
                    ?? throw new Exception($"failed to deserialize {nameof(Account.Created_V1)}");
                
                WriteLine("ACCOUNT CREATED");
                
                using var scope = _scopeFactory.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<Context>();

                var existing =
                    await context.Accounts.FirstOrDefaultAsync(
                        _ => _.UserId == e.data.public_id,
                        cancellationToken: stoppingToken
                    );

                if (existing != null) { 
                    WriteLine($"account already exists {existing.UserId}");
                    continue;
                }

                var account = new TaskServiceAccount
                {
                    UserId = e.data.public_id,
                    UserName = e.data.full_name,
                    Role = e.data.position
                };
                
                await context.Accounts.AddAsync(account, cancellationToken: stoppingToken);
                await context.SaveChangesAsync(cancellationToken: stoppingToken);
            }
            else if (eventName == Account.Updated_V1.Kind)
            {
                var e = 
                    JsonSerializer.Deserialize<Account.Updated_V1>(consumeResult.Message.Value, Json.Options)
                    ?? throw new Exception($"failed to deserialize {nameof(Account.Updated_V1)}");
                
                WriteLine("ACCOUNT UPDATED");
                
                using var scope = _scopeFactory.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<Context>();

                var existing =
                    await context.Accounts.FirstOrDefaultAsync(
                        _ => _.UserId == e.data.public_id,
                        cancellationToken: stoppingToken
                    );
                
                if (existing == null)
                    throw new Exception($"account {e.data.public_id} not found to update");

                existing.UserName = e.data.full_name;
                existing.Role = e.data.position;
                await context.SaveChangesAsync(cancellationToken: stoppingToken);
            }
            else if (eventName == Account.Deleted_V1.Kind)
            {
                var e = 
                    JsonSerializer.Deserialize<Account.Deleted_V1>(consumeResult.Message.Value, Json.Options)
                    ?? throw new Exception($"failed to deserialize {nameof(Account.Deleted_V1)}");
                
                WriteLine("ACCOUNT DELETED");
                
                using var scope = _scopeFactory.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<Context>();

                var existing =
                    await context.Accounts.FirstOrDefaultAsync(
                        _ => _.UserId == e.data.public_id,
                        cancellationToken: stoppingToken
                    );
                
                if (existing == null)
                    throw new Exception($"account {e.data.public_id} not found to delete");
                
                context.Accounts.Remove(existing);
                await context.SaveChangesAsync(cancellationToken: stoppingToken);
            }
        }

        consumer.Close();
    }
}