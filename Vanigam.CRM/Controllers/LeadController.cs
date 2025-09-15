using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanigam.CRM.Data;
using Vanigam.CRM.Shared.Models;

namespace Vanigam.CRM.Controllers;

public class LeadController(ApplicationDbContext context, ILogger<LeadController> logger)
    : BaseApiController<Lead, long>(context, logger)
{
    protected override DbSet<Lead> DbSet => Context.Lead;
    protected override Expression<Func<Lead, long>> KeySelector => x => x.Id ?? 0;
    protected override string EntityName => "Lead";

    protected override IQueryable<Lead> GetQueryWithIncludes()
    {
        return Context.Lead.Include(x => x.Address).Include(x => x.Opportunity).Include(x => x.Contact);
    }
}
