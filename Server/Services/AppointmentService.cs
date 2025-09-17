using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class AppointmentService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Appointment>> logger)
    : BaseService<Appointment>(context, logger)
{
    public override DbSet<Appointment> GetDbSet()
    {
        return Context.Appointments;
    }
}