using System.Net;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Session
{
    public class SessionValidationMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            if (context?.User?.Identity!=null && context.User.Identity.IsAuthenticated)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await userManager.FindByIdAsync(userId);
                var currentSessionId = context.User.FindFirstValue(nameof(ApplicationUser.SessionId));
                if (currentSessionId == null)
                {
                    await next(context); // Continue with the request if SessionId is not yet set
                    return;
                }
                else if (user?.SessionId != currentSessionId)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden; // Unauthorized
                    return;
                }
            }

            await next(context);
        }
    }

}

