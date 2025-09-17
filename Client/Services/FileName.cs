using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Vanigam.CRM.Client.Services
{
    public class ApplicationAuthorizeFilter(AuthorizationPolicy policy) : AuthorizeFilter(policy)
    {
        public override Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.HttpContext.Request.Path.StartsWithSegments("/Account") || context.HttpContext.Request.Path.StartsWithSegments("/Login"))
            {
                return Task.CompletedTask;
            }

            return base.OnAuthorizationAsync(context);
        }
    }
}

