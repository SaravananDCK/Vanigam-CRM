using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class VendorService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Vendor>> logger)
    : BaseService<Vendor>(context, logger)
{
    public override DbSet<Vendor> GetDbSet()
    {
        return Context.Vendors;
    }
    protected override async Task OnCreatedAsync(Vendor item)
    {
        if (string.IsNullOrEmpty(item.Code))
        {
            item.Code = await Context.GenerateNextCode(nameof(Vendor), TenantId);
        }
    }
}
