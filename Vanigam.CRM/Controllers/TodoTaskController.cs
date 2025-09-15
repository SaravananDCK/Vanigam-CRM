using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanigam.CRM.Data;
using Vanigam.CRM.Shared.Models;

namespace Vanigam.CRM.Controllers;

public class TodoTaskController(ApplicationDbContext context, ILogger<TodoTaskController> logger)
    : BaseApiController<TodoTask, long>(context, logger)
{
    protected override DbSet<TodoTask> DbSet => Context.TodoTask;
    protected override Expression<Func<TodoTask, long>> KeySelector => x => x.Id ?? 0;
    protected override string EntityName => "TodoTask";
}
