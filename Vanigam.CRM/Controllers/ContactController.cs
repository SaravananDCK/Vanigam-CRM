using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanigam.CRM.Data;
using Vanigam.CRM.Shared.Models;

namespace Vanigam.CRM.Controllers;

public class ContactController(ApplicationDbContext context, ILogger<ContactController> logger)
    : BaseApiController<Contact, long>(context, logger)
{
    protected override DbSet<Contact> DbSet => Context.Contact;
    protected override Expression<Func<Contact, long>> KeySelector => x => x.Id ?? 0;
    protected override string EntityName => "Contact";

    protected override IQueryable<Contact> GetQueryWithIncludes()
    {
        return Context.Contact.Include(x => x.Address).Include(x => x.Reward);
    }

    protected override async Task OnBeforeCreateAsync(Contact contact)
    {
        var reward = contact.Reward;
        contact.Reward = null;

        if (reward != null)
        {
            var newValues = await Context.Reward
                .Where(x => reward.Select(y => y.Id).Contains(x.Id))
                .ToListAsync();
            contact.Reward = [..newValues];
        }
    }

    protected override async Task OnBeforeUpdateAsync(Contact existing, Contact update)
    {
        await Context.Entry(existing).Collection(x => x.Reward).LoadAsync();

        if (update.Reward != null)
        {
            var updateValues = update.Reward.Select(x => x.Id);
            existing.Reward ??= [];
            existing.Reward.RemoveAll(x => !updateValues.Contains(x.Id));
            var addValues = updateValues.Where(x => !existing.Reward.Select(y => y.Id).Contains(x));
            var newValues = await Context.Reward.Where(x => addValues.Contains(x.Id)).ToListAsync();
            existing.Reward.AddRange(newValues);
        }
    }
}
