using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class EmployeeService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Employee>> logger)
    : BaseService<Employee>(context, logger)
{
    public override DbSet<Employee> GetDbSet()
    {
        return Context.Set<Employee>();
    }
}