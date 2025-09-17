using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Server.Services;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class LanguageService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Language>> logger)
    : BaseService<Language>(context, logger)
{
    public override DbSet<Language> GetDbSet()
    {
        return Context.Languages;
    }

    public override async Task<bool?> IsUnique(Language item)
    {
        return await Context?.Languages?.CountAsync(d => d.Oid != item.Oid && d.Code == item.Code) == 0; 
    }
}
