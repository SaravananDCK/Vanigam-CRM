using Hangfire.Dashboard;

namespace Vanigam.CRM.Server.HangFire
{
    public class NoAuthorizationFilter : IDashboardAuthorizationFilter
    {

        public NoAuthorizationFilter()
        {

        }

        public bool Authorize(DashboardContext dashboardContext)
        {
            return true;
        }
    }
}

