using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanigam.CRM.Data;
using Vanigam.CRM.Shared.Models;

namespace Vanigam.CRM.Controllers;

public class CustomerController(ApplicationDbContext context, ILogger<CustomerController> logger)
    : BaseApiController<Customer, long>(context, logger)
{
    protected override DbSet<Customer> DbSet => Context.Customer;
    protected override Expression<Func<Customer, long>> KeySelector => x => x.Id ?? 0;
    protected override string EntityName => "Customer";

    protected override IQueryable<Customer> GetQueryWithIncludes()
    {
        return Context.Customer.Include(x => x.Address).Include(x => x.Contact);
    }

    protected override async Task OnBeforeCreateAsync(Customer customer)
    {
        var contact = customer.Contact;
        customer.Contact = null;

        if (contact != null)
        {
            var newValues = await Context.Contact
                .Where(x => contact.Select(y => y.Id).Contains(x.Id))
                .ToListAsync();
            customer.Contact = [..newValues];
        }
    }

    protected override async Task OnBeforeUpdateAsync(Customer existing, Customer update)
    {
        await Context.Entry(existing).Collection(x => x.Contact).LoadAsync();

        if (update.Contact != null)
        {
            var updateValues = update.Contact.Select(x => x.Id);
            existing.Contact ??= [];
            existing.Contact.RemoveAll(x => !updateValues.Contains(x.Id));
            var addValues = updateValues.Where(x => !existing.Contact.Select(y => y.Id).Contains(x));
            var newValues = await Context.Contact.Where(x => addValues.Contains(x.Id)).ToListAsync();
            existing.Contact.AddRange(newValues);
        }
    }
}
