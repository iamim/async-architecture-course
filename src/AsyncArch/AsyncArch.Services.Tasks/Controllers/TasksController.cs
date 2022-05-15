using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AsyncArch.Schema;
using AsyncArch.Services.Tasks.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task = AsyncArch.Schema.Events.Task;

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
        [FromQuery] Guid claim_user,
        [FromServices] Context ctx,
        [FromServices] KafkaProducer producer,
        [FromServices] ILogger<TasksController> logger
    )
    {
        if (!await ctx.Accounts.AnyAsync(a => a.UserId == claim_user))
            return Unauthorized();

        var to = await ctx.Accounts.FirstOrDefaultAsync(_ => _.UserId == assignee);
        if (to == null)
            return BadRequest("Assignee not found");

        var db = new Db.Models.Task
        {
            Assignee = assignee,
            Description = description,
            IsDone = done
        };

        var msg = new Task.Created_V1(
            event_id: Guid.NewGuid().ToString(),
            event_version: 1,
            event_name: Task.Created_V1.Kind,
            event_time: DateTimeOffset.Now,
            producer: "TasksService",
            data: new Task.Created_V1.Data(
                task_uuid: Guid.NewGuid(),
                assignee: assignee,
                title: title,
                description: description,
                done: done
            )
        );

        await ctx.Tasks.AddAsync(db);

        await producer.Send(
            KafkaProducer.BusinessTopic,
            JsonSerializer.Serialize(msg, Json.Options)
        );

        await ctx.SaveChangesAsync();

        return Ok(msg.data.task_uuid);
    }

    [HttpPost("shuffle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Shuffle(
        [FromQuery] Guid claim_user,
        [FromServices] Context ctx,
        [FromServices] KafkaProducer producer,
        [FromServices] ILogger<TasksController> logger
    )
    {
        var user = await ctx.Accounts.FirstOrDefaultAsync(_ => _.UserId == claim_user);
        if (user is null || user.Role != "admin")
            return Unauthorized();

        var undones = await ctx.Tasks.Where(_ => !_.IsDone).ToListAsync();
        var workers = await ctx.Accounts.Where(_ => _.Role == "worker").ToListAsync();

        var rnd = new Random();
        var reassignes = new List<Task.Reassigned_V1>();
        foreach (var undone in undones)
        {
            var pick = workers[rnd.Next(workers.Count)];

            var was = undone.Assignee;
            undone.Assignee = pick.UserId;

            var e = new Task.Reassigned_V1(
                event_id: Guid.NewGuid().ToString(),
                event_version: 1,
                event_name: Task.Reassigned_V1.Kind,
                event_time: DateTimeOffset.Now,
                producer: "TasksService",
                data: new Task.Reassigned_V1.Data(
                    task_uuid: undone.Uuid,
                    was_assignee_uuid: was,
                    now_assignee_uuid: undone.Assignee,
                    task_description: undone.Description
                )
            );

            reassignes.Add(e);
        }

        await producer.Send(
            KafkaProducer.BusinessTopic,
            reassignes.Select(_ => JsonSerializer.Serialize(_, Json.Options)).ToArray()
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
        [FromServices] KafkaProducer producer,
        [FromServices] ILogger<TasksController> logger
    )
    {
        var task =
            await ctx.Tasks.FirstOrDefaultAsync(_ => _.Uuid == task_uuid && !_.IsDone && _.Assignee == claim_user);

        if (task == null)
            return BadRequest();

        task.IsDone = true;

        var e = new Task.Completed_V1(
            event_id: Guid.NewGuid().ToString(),
            event_version: 1,
            event_name: Task.Completed_V1.Kind,
            event_time: DateTimeOffset.Now,
            producer: "TasksService",
            data: new Task.Completed_V1.Data(
                task_uuid: task.Uuid
            )
        );

        await producer.Send(
            KafkaProducer.BusinessTopic,
            JsonSerializer.Serialize(e, Json.Options)
        );

        await ctx.SaveChangesAsync();

        return Ok();
    }
}