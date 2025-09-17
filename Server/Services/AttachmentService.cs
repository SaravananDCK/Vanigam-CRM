using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class AttachmentService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Attachment>> logger)
    : BaseService<Attachment>(context, logger)
{
    public override DbSet<Attachment> GetDbSet()
    {
        return Context.Attachments;
    }
}