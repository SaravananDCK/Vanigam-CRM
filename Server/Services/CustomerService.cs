using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class CustomerService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Customer>> logger)
    : BaseService<Customer>(context, logger)
{
    public override DbSet<Customer> GetDbSet()
    {
        return Context.Customers;
    }
}


