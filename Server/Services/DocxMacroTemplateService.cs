using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Services;

public class DocxMacroTemplateService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<DocxMacroTemplate>> logger)
    : BaseService<DocxMacroTemplate>(context, logger)
{
    public override DbSet<DocxMacroTemplate> GetDbSet()
    {
        return Context.DocxMacroTemplates;
    }
}

