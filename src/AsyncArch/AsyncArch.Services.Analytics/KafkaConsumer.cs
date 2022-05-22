using System.Text.Json;
using AsyncArch.Schema;
using AsyncArch.Schema.Events;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;

namespace AsyncArch.Services.Analytics;

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
            GroupId = "analytics-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();

        consumer.Subscribe(new[] {"accounts", "accounts-stream", "tasks", "tasks-stream"});

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

            if (eventName == Accounts.RoleChanged_V1.Kind && eventVersion == 1) 
            {
                var e = 
                    JsonSerializer.Deserialize<Accounts.RoleChanged_V1>(consumeResult.Message.Value, Json.Options)
                    ?? throw new Exception($"failed to deserialize {nameof(Accounts.RoleChanged_V1)}");
                
                using var scope = _scopeFactory.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<Db.Context>();

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
            else if (eventName == Accounts.Created_V1.Kind && eventVersion == 1)
            {
                var e = 
                    JsonSerializer.Deserialize<Accounts.Created_V1>(consumeResult.Message.Value, Json.Options)
                    ?? throw new Exception($"failed to deserialize {nameof(Accounts.Created_V1)}");
                
                using var scope = _scopeFactory.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<Db.Context>();

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

                var account = new Db.Models.Account
                {
                    AccountGuid = e.data.public_id,
                    Name = e.data.full_name,
                    Role = e.data.position
                };
                
                await context.Accounts.AddAsync(account, cancellationToken: stoppingToken);
                await context.SaveChangesAsync(CancellationToken.None);
            }
            else if (eventName == Accounts.Updated_V1.Kind && eventVersion == 1)
            {
                var e = 
                    JsonSerializer.Deserialize<Accounts.Updated_V1>(consumeResult.Message.Value, Json.Options)
                    ?? throw new Exception($"failed to deserialize {nameof(Accounts.Updated_V1)}");
                
                using var scope = _scopeFactory.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<Db.Context>();

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
            else if (eventName == Accounts.Deleted_V1.Kind && eventVersion == 1)
            {
                var e = 
                    JsonSerializer.Deserialize<Accounts.Deleted_V1>(consumeResult.Message.Value, Json.Options)
                    ?? throw new Exception($"failed to deserialize {nameof(Accounts.Deleted_V1)}");
                
                using var scope = _scopeFactory.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<Db.Context>();

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
            // else if (eventName == Tasks.Created_V2.Kind && eventVersion == 2) 
            // { 
            //     var e = 
            //         JsonSerializer.Deserialize<Tasks.Created_V2>(consumeResult.Message.Value, Json.Options)
            //         ?? throw new Exception($"failed to deserialize {nameof(Tasks.Created_V2)}");
            //     
            //     using var scope = _scopeFactory.CreateScope();
            //     await using var context = scope.ServiceProvider.GetRequiredService<Db.Context>();
            //
            //     var rnd = new Random();
            //     var pricing = new Db.Models.Task
            //     {
            //         TaskGuid = e.data.task_uuid,
            //         
            //         JiraId = e.data.jira_id,
            //         Description = e.data.description,
            //         
            //         AssignmentDeduction = (uint) rnd.Next(10, 20),
            //         CompletionBonus = (uint) rnd.Next(20, 40)
            //     };
            //
            //     var balanceTransaction = new Db.Models.AccountBalanceTransaction
            //     {
            //         Time = DateTimeOffset.Now,
            //         AccountGuid = e.data.assignee,
            //         RelatedToTaskGuid = e.data.task_uuid,
            //         Explanation = Db.Models.AccountBalanceTransaction.Reason.Assigned,
            //         BalanceChange = -(int) pricing.AssignmentDeduction
            //     };
            //
            //     context.Tasks.Add(pricing);
            //     context.AccountBalanceTransactions.Add(balanceTransaction);
            //
            //     await context.SaveChangesAsync(CancellationToken.None);
            // }
            // else if (eventName == Tasks.Reassigned_V1.Kind && eventVersion == 1) 
            // { 
            //     var e = 
            //         JsonSerializer.Deserialize<Tasks.Reassigned_V1>(consumeResult.Message.Value, Json.Options)
            //         ?? throw new Exception($"failed to deserialize {nameof(Tasks.Reassigned_V1)}");
            //     
            //     using var scope = _scopeFactory.CreateScope();
            //     await using var context = scope.ServiceProvider.GetRequiredService<Db.Context>();
            //
            //     Db.Models.Task pricing;
            //     if (await context.Tasks
            //             .FirstOrDefaultAsync(
            //                 _ => _.TaskGuid == e.data.task_uuid,
            //                 cancellationToken: stoppingToken
            //             ) is { } existing)
            //     {
            //         pricing = existing;
            //     }
            //     else
            //     {
            //         var rnd = new Random();
            //         pricing = new Db.Models.Task
            //         {
            //             TaskGuid = e.data.task_uuid,
            //             AssignmentDeduction = (uint) rnd.Next(10, 20),
            //             CompletionBonus = (uint) rnd.Next(20, 40)
            //         };
            //         
            //         context.Tasks.Add(pricing);
            //     }
            //
            //     var balanceTransaction = new Db.Models.AccountBalanceTransaction
            //     {
            //         Time = DateTimeOffset.Now, 
            //         AccountGuid = e.data.now_assignee_uuid,
            //         RelatedToTaskGuid = e.data.task_uuid,
            //         Explanation = Db.Models.AccountBalanceTransaction.Reason.Assigned,
            //         BalanceChange = -(int) pricing.AssignmentDeduction
            //     };
            //
            //     context.AccountBalanceTransactions.Add(balanceTransaction);
            //
            //     await context.SaveChangesAsync(CancellationToken.None);
            // }
            // else if (eventName == Tasks.Completed_V1.Kind && eventVersion == 1) 
            // { 
            //     var e = 
            //         JsonSerializer.Deserialize<Tasks.Completed_V1>(consumeResult.Message.Value, Json.Options)
            //         ?? throw new Exception($"failed to deserialize {nameof(Tasks.Completed_V1)}");
            //     
            //     using var scope = _scopeFactory.CreateScope();
            //     await using var context = scope.ServiceProvider.GetRequiredService<Db.Context>();
            //
            //     Db.Models.Task pricing;
            //     if (await context.Tasks
            //             .FirstOrDefaultAsync(
            //                 _ => _.TaskGuid == e.data.task_uuid,
            //                 cancellationToken: stoppingToken
            //             ) is { } existing)
            //     {
            //         pricing = existing;
            //     }
            //     else
            //     {
            //         var rnd = new Random();
            //         pricing = new Db.Models.Task
            //         {
            //             TaskGuid = e.data.task_uuid,
            //             AssignmentDeduction = (uint) rnd.Next(10, 20),
            //             CompletionBonus = (uint) rnd.Next(20, 40)
            //         };
            //         
            //         context.Tasks.Add(pricing);
            //     }
            //
            //     var balanceTransaction = new Db.Models.AccountBalanceTransaction
            //     {
            //         Time = DateTimeOffset.Now,
            //         AccountGuid = e.data.assignee_uuid,
            //         RelatedToTaskGuid = e.data.task_uuid,
            //         Explanation = Db.Models.AccountBalanceTransaction.Reason.Assigned,
            //         BalanceChange = -(int) pricing.AssignmentDeduction
            //     };
            //
            //     context.AccountBalanceTransactions.Add(balanceTransaction);
            //
            //     await context.SaveChangesAsync(CancellationToken.None);
            // }
        }

        consumer.Close();
    }
}