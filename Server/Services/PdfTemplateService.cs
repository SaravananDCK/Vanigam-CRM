using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Services;

public class PdfTemplateService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<PdfTemplate>> logger)
    : BaseService<PdfTemplate>(context, logger)
{
    public override DbSet<PdfTemplate> GetDbSet()
    {
        return Context.PdfTemplates;
    }
}
