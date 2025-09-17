using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class QuoteService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Quote>> logger)
    : BaseService<Quote>(context, logger)
{
    public override DbSet<Quote> GetDbSet()
    {
        return Context.Quotes;
    }
}