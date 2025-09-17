using Microsoft.EntityFrameworkCore;
using Vanigam.Server.Controllers;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class ReportTemplateService(VanigamAccountingDbContext context, ILogger<BaseService<ReportTemplate>> logger)
: BaseService<ReportTemplate>(context, logger)
{
    public override DbSet<ReportTemplate> GetDbSet()
    {
        return context.ReportTemplates;
    }
}

