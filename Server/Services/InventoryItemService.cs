using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class InventoryItemService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<InventoryItem>> logger)
    : BaseService<InventoryItem>(context, logger)
{
    public override DbSet<InventoryItem> GetDbSet()
    {
        return Context.InventoryItems;
    }
}