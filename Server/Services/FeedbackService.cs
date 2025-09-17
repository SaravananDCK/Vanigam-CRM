using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class FeedbackService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Feedback>> logger)
    : BaseService<Feedback>(context, logger)
{
    public override DbSet<Feedback> GetDbSet()
    {
        return Context.Feedbacks;
    }
}