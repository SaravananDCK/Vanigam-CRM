using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanigam.CRM.Data;
using Vanigam.CRM.Shared.Models;

namespace Vanigam.CRM.Controllers;

public class SaleController(ApplicationDbContext context, ILogger<SaleController> logger)
    : BaseApiController<Sale, long>(context, logger)
{
    protected override DbSet<Sale> DbSet => Context.Sale;
    protected override Expression<Func<Sale, long>> KeySelector => x => x.Id ?? 0;
    protected override string EntityName => "Sale";
}
