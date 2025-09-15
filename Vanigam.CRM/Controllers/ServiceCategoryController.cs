using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanigam.CRM.Data;
using Vanigam.CRM.Shared.Models;

namespace Vanigam.CRM.Controllers;

public class ServiceCategoryController(ApplicationDbContext context, ILogger<ServiceCategoryController> logger)
    : BaseApiController<ServiceCategory, long>(context, logger)
{
    protected override DbSet<ServiceCategory> DbSet => Context.ServiceCategory;
    protected override Expression<Func<ServiceCategory, long>> KeySelector => x => x.Id ?? 0;
    protected override string EntityName => "ServiceCategory";
}
