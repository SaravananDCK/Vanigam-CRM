using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class TaxCodeService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<TaxCode>> logger)
    : BaseService<TaxCode>(context, logger)
{
    public override DbSet<TaxCode> GetDbSet()
    {
        return Context.TaxCodes;
    }
}
