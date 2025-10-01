using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class VoucherLineService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<VoucherLine>> logger)
    : BaseService<VoucherLine>(context, logger)
{
    public override DbSet<VoucherLine> GetDbSet()
    {
        return Context.VoucherLines;
    }
}
