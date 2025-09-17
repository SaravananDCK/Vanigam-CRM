using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Services;

public class FileCategoryService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<FileCategory>> logger)
    : BaseService<FileCategory>(context, logger)
{
    public override DbSet<FileCategory> GetDbSet()
    {
        return Context.FileCategories;
    }
}
