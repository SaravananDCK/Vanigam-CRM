using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class ItemService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Item>> logger)
    : BaseService<Item>(context, logger)
{
    public override DbSet<Item> GetDbSet()
    {
        return Context.Items;
    }
}