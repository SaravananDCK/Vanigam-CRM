using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanigam.CRM.Data;
using Vanigam.CRM.Shared.Models;

namespace Vanigam.CRM.Controllers;

public class OpportunityController(ApplicationDbContext context, ILogger<OpportunityController> logger)
    : BaseApiController<Opportunity, long>(context, logger)
{
    protected override DbSet<Opportunity> DbSet => Context.Opportunity;
    protected override Expression<Func<Opportunity, long>> KeySelector => x => x.Id ?? 0;
    protected override string EntityName => "Opportunity";
}
