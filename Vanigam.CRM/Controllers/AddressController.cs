using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanigam.CRM.Data;
using Vanigam.CRM.Shared.Models;

namespace Vanigam.CRM.Controllers;

public class AddressController(ApplicationDbContext context, ILogger<AddressController> logger)
    : BaseApiController<Address, long>(context, logger)
{
    protected override DbSet<Address> DbSet => Context.Address;
    protected override Expression<Func<Address, long>> KeySelector => x => x.Id ?? 0;
    protected override string EntityName => "Address";
}
