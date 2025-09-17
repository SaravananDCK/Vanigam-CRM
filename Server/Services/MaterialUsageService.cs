using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class MaterialUsageService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<MaterialUsage>> logger)
    : BaseService<MaterialUsage>(context, logger)
{
    public override DbSet<MaterialUsage> GetDbSet()
    {
        return Context.MaterialUsages;
    }
}