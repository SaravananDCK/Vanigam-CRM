using DocumentFormat.OpenXml.InkML;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Redis;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.DTOs;

namespace Vanigam.CRM.Server.Permissions
{
    public static class PermissionClaim
    {
        public static async Task AddPermissionClaim(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, 
            ApplicationUser user, RedisService redisService)
        {
                var roles = user.Roles.Select(role => role.Id);
                IList<Claim> roleClaims = new List<Claim>();
                var priority = RolePriority.Eleventh;

                foreach (var roleId in roles)
                {
                    var role = await roleManager.FindByIdAsync(roleId.ToString());
                    if (role != null)
                    {
                        var claims = await roleManager.GetClaimsAsync(role);
                        var claimData = claims.FirstOrDefault();

                        if (claimData != null)
                        {
                            var data = JsonSerializer.Deserialize<AuthorizationPermission>(claimData.Value);
                            if (data.Priority < priority)
                            {
                                priority = data.Priority;
                                roleClaims = claims;
                            }
                        }
                    }
                }
                //Assign Data in Redis
                await redisService.StoreUserClaimsAsync(user.Id.ToString(), roleClaims);
            }
    }
}

