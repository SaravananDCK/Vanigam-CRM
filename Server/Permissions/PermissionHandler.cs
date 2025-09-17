using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Vanigam.CRM.Objects.Redis;
using Vanigam.CRM.Objects.DTOs;

namespace Vanigam.CRM.Server.Permissions
{
    public class PermissionHandler(RedisService redisService) : AuthorizationHandler<PermissionRequirement>
    {
        protected override async Task<Task> HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var httpContext = context.Resource as HttpContext;
            var routeData = httpContext?.GetRouteData();
            var controllerName = routeData?.Values["controller"]?.ToString();

            // Handle case when the controller name is not available (e.g., during a refresh)
            if (controllerName == null)
            {
                if (httpContext?.Request.Path != null)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
            var userId = context.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                context.Fail(new AuthorizationFailureReason(this,"User ID not found in claims."));
                // For example, throw an exception or return an error response
                throw new InvalidOperationException("User ID not found in claims.");

            }

            var claims = await redisService.GetUserClaimsAsync(userId.ToString());

            var readPermissions = claims.Where(c => c.Type == "Permission." + controllerName)
                .Select(c => JsonSerializer.Deserialize<AuthorizationPermission>(c.Value)).ToList();

            bool hasPermission = readPermissions.Any(role =>
                (requirement.Permission switch
                {
                    "IsRead" => role.Read,
                    "IsCreate" => role.Create,
                    "IsUpdate" => role.Update,
                    "IsDelete" => role.Delete,
                    _ => false
                }));
            
            if (hasPermission)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}

