using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AsyncArch.Common;
using AsyncArch.Services.Tasks.Db;
using AsyncArch.Services.Tasks.Db.Models;
using AsyncArch.Services.Tasks.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AsyncArch.Services.Tasks.Controllers;

[ApiController]
[Route("[controller]")]
public class TasksController : ControllerBase
{
    [HttpPost("create-task")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateTask(
        [FromQuery, Required] Guid assignee,
        [FromQuery, Required] string title,
        [FromQuery, Required] string description,
        [FromQuery, Required] bool done,
        [FromServices] Context ctx,
        [FromServices] Producer producer,
        [FromServices] ILogger<TasksController> logger
    )
    {
        var to = await ctx.Accounts.FirstOrDefaultAsync(_ => _.UserId == assignee);
        if (to == null)
            return BadRequest("Assignee not found");

        var db = new TaskServiceTask
        {
            Assignee = assignee,
            Description = description,
            IsDone = done
        };

        var e = new TaskCreated
        {
            task_uuid = Guid.NewGuid(),
            assignee = assignee,
            title = title,
            description = description,
            done = done
        };
        
        await ctx.Tasks.AddAsync(db);
        
        await producer.SendEvent(
            Producer.BusinessTopic,
            JsonSerializer.Serialize(e, Json.Options)
        );
        
        await ctx.SaveChangesAsync();
        
        return Ok(e.task_uuid);
    }
    
    [HttpPost("shuffle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Shuffle(
        [FromQuery] Guid claim_user,
        [FromServices] Context ctx,
        [FromServices] Producer producer,
        [FromServices] ILogger<TasksController> logger
    )
    {
        var user = await ctx.Accounts.FirstOrDefaultAsync(_ => _.UserId == claim_user);
        if (user is null || user.Role != "admin")
            return Unauthorized();
        
        var undones = await ctx.Tasks.Where(_ => !_.IsDone).ToListAsync();
        var workers = await ctx.Accounts.Where(_ => _.Role == "worker").ToListAsync();
        
        var rnd = new Random();
        var reassignes = new List<TaskReAssigned>();
        foreach (var undone in undones)
        {
            var pick = workers[rnd.Next(workers.Count)];

            var was = undone.Assignee;
            undone.Assignee = pick.UserId;

            reassignes.Add(new TaskReAssigned
            {
                task_uuid = undone.Uuid,
                was_assignee_uuid = was,
                now_assignee_uuid = undone.Assignee,
                task_description = undone.Description
            });
        }
        
        foreach (var reassign in reassignes)
            await producer.SendEvent(
                Producer.BusinessTopic,
                JsonSerializer.Serialize(reassign, Json.Options)
            );
        
        await ctx.SaveChangesAsync();
        
        return Ok();
    }

    public record PrivateBoardResponse(Guid assignee, List<PrivateBoardResponse.Task> tasks)
    {
        public record Task(Guid task_uuid, string description, bool done);
    }

    [HttpGet("private-board")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PrivateBoardResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<PrivateBoardResponse> PrivateBoard(
        [FromQuery] Guid claim_user,
        [FromServices] Context ctx,
        [FromServices] ILogger<TasksController> logger
    )
    {
        var mine = await ctx.Tasks.Where(_ => _.Assignee == claim_user).ToListAsync();

        return new(
            Guid.Empty,
            mine.Select(_ => new PrivateBoardResponse.Task(_.Uuid, _.Description, _.IsDone)).ToList()
        );
    }

    [HttpPost("complete-task")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CompleteTask(
        [FromQuery, Required] Guid task_uuid,
        [FromQuery] Guid claim_user,
        [FromServices] Context ctx,
        [FromServices] Producer producer,
        [FromServices] ILogger<TasksController> logger
    )
    {
        var task =
            await ctx.Tasks.FirstOrDefaultAsync(_ => _.Uuid == task_uuid && !_.IsDone && _.Assignee == claim_user);

        if (task == null)
            return BadRequest();

        task.IsDone = true;

        var e = new TaskCompleted
        {
            task_uuid = task.Uuid,
            assignee_uuid = task.Assignee,
            task_description = task.Description
        };

        await producer.SendEvent(
            Producer.BusinessTopic,
            JsonSerializer.Serialize(e, Json.Options)
        );
        
        await ctx.SaveChangesAsync();
        
        return Ok();
    }
}