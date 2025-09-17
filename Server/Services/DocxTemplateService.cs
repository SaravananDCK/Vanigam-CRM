using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Services;

public class DocxTemplateService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<DocxTemplate>> logger)
    : BaseService<DocxTemplate>(context, logger)
{
    public override DbSet<DocxTemplate> GetDbSet()
    {
        return Context.DocxTemplates;
    }
}


