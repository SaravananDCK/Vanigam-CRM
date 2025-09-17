using Cosmos.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using Vanigam.CRM.Objects;

namespace Vanigam.CRM.Server.Permissions
{
    public class ReconcilePermissionService(
        VanigamAccountingDbContext context, 
        RoleManager<ApplicationRole> roleManager)
    {
        public async Task ReconcilePermission()
        {
            var roleClaims = new List<KeyValuePair<ApplicationRole, Claim>>();
            var roles = roleManager.Roles.ToList();
            var tableNames = GetTableNames();
            var uniqueClaimTypes = new HashSet<string>();

            foreach (var role in roles)
            {
                var claims = await roleManager.GetClaimsAsync(role);
                foreach (var claim in claims)
                {
                    // Storing Permissions for assigning values to new role Claims.
                    if (claim.Type == "Permission.Activities")
                        roleClaims.Add(new KeyValuePair<ApplicationRole, Claim>(role, claim));
                    
                    var claimTypeName = claim.Type.Split('.').Last();
                    uniqueClaimTypes.Add(claimTypeName);
                }
            }

            if (uniqueClaimTypes.Count() == tableNames.Count())
            {
                return;
            }

            // Find missing claim types for tables
            var missingClaimTypes = tableNames.Except(uniqueClaimTypes).ToList();
            
            // Add missing claim types to the Claims
            if (missingClaimTypes.Any())
            {
                var claimsToAdd = missingClaimTypes.SelectMany(claimType => roleClaims.Select(roleClaim =>
                new Claim("Permission." + claimType, roleClaim.Value.Value) { Properties = { { "Role", roleClaim.Key.Name } }})).ToList();

                foreach (var newClaim in claimsToAdd)
                {
                    var roleName = newClaim.Properties["Role"];
                    var role = await roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        var res = await roleManager.AddClaimAsync(role, newClaim);
                    }
                }
            }
            
        }

        public List<string> GetTableNames()
        {
            return context.GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(p => context.Model.FindEntityType(p.PropertyType.GetGenericArguments()[0])?.GetTableName())
                .Where(tableName => !string.IsNullOrEmpty(tableName))
                .ToList();
        }

    }
}

