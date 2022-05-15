using System.Text.Json;
using AsyncArch.Common;
using AsyncArch.Services.Tasks.Db;
using AsyncArch.Services.Tasks.Db.Models;
using AsyncArch.Services.Tasks.Events;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using static System.Console;

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

            if (eventName == AccountRoleChanged.Kind) 
            {
                var e = 
                    JsonSerializer.Deserialize<AccountRoleChanged>(consumeResult.Message.Value, Json.Options)
                    ?? throw new Exception($"failed to deserialize {nameof(AccountRoleChanged)}");
                
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
            else if (eventName == AccountCreated.Kind)
            {
                var e = 
                    JsonSerializer.Deserialize<AccountCreated>(consumeResult.Message.Value, Json.Options)
                    ?? throw new Exception($"failed to deserialize {nameof(AccountCreated)}");
                
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
            else if (eventName == AccountUpdated.Kind)
            {
                var e = 
                    JsonSerializer.Deserialize<AccountUpdated>(consumeResult.Message.Value, Json.Options)
                    ?? throw new Exception($"failed to deserialize {nameof(AccountUpdated)}");
                
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
            else if (eventName == AccountDeleted.Kind)
            {
                var e = 
                    JsonSerializer.Deserialize<AccountDeleted>(consumeResult.Message.Value, Json.Options)
                    ?? throw new Exception($"failed to deserialize {nameof(AccountDeleted)}");
                
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