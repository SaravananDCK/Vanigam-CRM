using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Services;

public class PdfFieldService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<PdfField>> logger)
    : BaseService<PdfField>(context, logger)
{
    public override DbSet<PdfField> GetDbSet()
    {
        return Context.PdfFields;
    }
}
