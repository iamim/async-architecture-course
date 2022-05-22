using System.ComponentModel.DataAnnotations;
using System.Text;
using AsyncArch.Services.Accounting.Db.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsyncArch.Services.Accounting.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountingController : ControllerBase
{
    public record AdminResponse(int total_earned);
    
    [HttpPost("admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminResponse>> Admin(
        [FromQuery] Guid claim_user,
        [FromServices] Db.Context ctx,
        [FromServices] KafkaProducer producer,
        [FromServices] ILogger<AccountingController> logger
    )
    {
        var found = await ctx.Accounts.FirstOrDefaultAsync(_ => _.AccountGuid == claim_user);
        if (found is not {Role: "admin" or "manager"})
            return Unauthorized();

        var today = DateTime.UtcNow.Date;
        var transactions = 
            await 
                ctx.AccountBalanceTransactions
                    .Where(_ => _.Time.Date == today)
                    .OrderByDescending(_ => _.Time)
                    .ToListAsync();

        var total = 0;
        
        foreach (var transaction in transactions)
        {
            if (transaction.Explanation is AccountBalanceTransaction.Reason.Assigned
                or AccountBalanceTransaction.Reason.Completed)
            {
                total += -transaction.BalanceChange;
            }
        }

        return new AdminResponse(total_earned: total);
    }

    public record WorkerResponse(int balance, int earned_today, List<string> today_log);
    
    [HttpPost("worker")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkerResponse>> Worker(
        [FromQuery] Guid claim_user,
        [FromServices] Db.Context ctx,
        [FromServices] KafkaProducer producer,
        [FromServices] ILogger<AccountingController> logger
    )
    {
        var today = DateTime.UtcNow.Date;
        var transactions = 
            await 
                ctx.AccountBalanceTransactions
                    .Where(_ => _.AccountGuid == claim_user)
                    .Where(_ => _.Time.Date == today)
                    .OrderByDescending(_ => _.Time)
                    .ToListAsync();

        var earned = 0;
        var log = new List<string>();
        foreach (var transaction in transactions)
        {
            if (transaction.Explanation is 
                AccountBalanceTransaction.Reason.Assigned or AccountBalanceTransaction.Reason.Completed)
            {
                earned += transaction.BalanceChange;

                var sb = new StringBuilder();
                
                sb.Append($"{transaction.Time:yyyy-MM-dd HH:mm:ss}");
                sb.Append($" ${transaction.BalanceChange}");
                sb.Append($" why:{transaction.Explanation.ToString("G")}");
                
                // TODO: get from join
                var task = await ctx.Tasks.FirstOrDefaultAsync(_ => _.TaskGuid == transaction.RelatedToTaskGuid);
                if (task is null)
                {
                    sb.Append(" task:null");
                }
                else
                {
                    sb.Append($" task:[{task.JiraId ?? "unknown_jira"}] {task.Description ?? "unknown_description"}");
                }

                log.Add(sb.ToString());
            }
        }

        // TODO: add balance
        return new WorkerResponse(
            balance: 0,
            earned_today: earned,
            today_log: log
        );
    }
}