using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanigam.CRM.Data;
using Vanigam.CRM.Shared.Models;

namespace Vanigam.CRM.Controllers;

public class RewardController(ApplicationDbContext context, ILogger<RewardController> logger)
    : BaseApiController<Reward, long>(context, logger)
{
    protected override DbSet<Reward> DbSet => Context.Reward;
    protected override Expression<Func<Reward, long>> KeySelector => x => x.Id ?? 0;
    protected override string EntityName => "Reward";
}
