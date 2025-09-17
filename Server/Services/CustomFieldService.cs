using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class CustomFieldService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<CustomField>> logger)
    : BaseService<CustomField>(context, logger)
{
    public override DbSet<CustomField> GetDbSet()
    {
        return Context.CustomFields;
    }
}