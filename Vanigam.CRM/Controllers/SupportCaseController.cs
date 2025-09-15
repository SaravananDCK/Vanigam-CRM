using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanigam.CRM.Data;
using Vanigam.CRM.Shared.Models;

namespace Vanigam.CRM.Controllers;

public class SupportCaseController(ApplicationDbContext context, ILogger<SupportCaseController> logger)
    : BaseApiController<SupportCase, long>(context, logger)
{
    protected override DbSet<SupportCase> DbSet => Context.SupportCase;
    protected override Expression<Func<SupportCase, long>> KeySelector => x => x.Id ?? 0;
    protected override string EntityName => "SupportCase";
}
