using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class QuoteItemService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<QuoteItem>> logger)
    : BaseService<QuoteItem>(context, logger)
{
    public override DbSet<QuoteItem> GetDbSet()
    {
        return Context.QuoteItems;
    }
}