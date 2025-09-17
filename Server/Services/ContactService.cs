using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class ContactService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Contact>> logger)
    : BaseService<Contact>(context, logger)
{
    public override DbSet<Contact> GetDbSet()
    {
        return Context.Contacts;
    }
}