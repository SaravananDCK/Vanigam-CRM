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
            var userId = context.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                context.Fail(new AuthorizationFailureReason(this, "User ID not found in claims."));
                return Task.CompletedTask;
            }

            // Handle permission-based requirements (e.g., "Permission.Customers")
            if (requirement.Permission.StartsWith("Permission.", StringComparison.OrdinalIgnoreCase))
            {
                // Check if user has the specific permission claim
                var hasPermissionClaim = context.User.HasClaim(requirement.Permission, requirement.Permission);
                if (hasPermissionClaim)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                // Also check for role-based permissions
                var roleClaims = context.User.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value);

                // Check if user has permission through role claims stored in database
                var claims = await redisService.GetUserClaimsAsync(userId.ToString());
                var permissionClaims = claims.Where(c => c.Type == requirement.Permission);

                if (permissionClaims.Any())
                {
                    foreach (var claim in permissionClaims)
                    {
                        try
                        {
                            var permission = JsonSerializer.Deserialize<AuthorizationPermission>(claim.Value);
                            if (permission != null && permission.Read) // For now, just check Read permission
                            {
                                context.Succeed(requirement);
                                return Task.CompletedTask;
                            }
                        }
                        catch
                        {
                            // Continue to next claim if parsing fails
                        }
                    }
                }
            }
            // Handle legacy "Is" permissions
            else if (requirement.Permission.StartsWith("Is", StringComparison.OrdinalIgnoreCase))
            {
                var httpContext = context.Resource as HttpContext;
                var routeData = httpContext?.GetRouteData();
                var controllerName = routeData?.Values["controller"]?.ToString();

                if (controllerName == null && httpContext?.Request.Path != null)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
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
            }

            return Task.CompletedTask;
        }
    }
}

