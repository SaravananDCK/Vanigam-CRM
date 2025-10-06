using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class ItemCategoryService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<ItemCategory>> logger)
    : BaseService<ItemCategory>(context, logger)
{
    public override DbSet<ItemCategory> GetDbSet()
    {
        return Context.ItemCategories;
    }
}
