using DevExpress.Web.Internal.Dialogs;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;

namespace Vanigam.CRM.Server.Permissions
{
    public class PermissionService(RoleManager<ApplicationRole> roleManager)
    {
        public async Task<IEnumerable<AuthorizationPermission>> GetClaims(string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return null;
            }
            var claims = await roleManager.GetClaimsAsync(role);
            var orderedClaims = claims.OrderBy(c => c.Type);
            var permissions = orderedClaims.Select(item =>
            {
                var permission = JsonSerializer.Deserialize<AuthorizationPermission>(item.Value);
            if (permission != null)
            {
                permission.RoleId = roleId;
                permission.ClaimType = item.Type.Split('.').Last(); // Set ClaimType from the claim's type
            }
            return permission;
           }).Where(p => p != null);

           return permissions;
        }

       
        public async Task<bool> UpdatePermissions(AuthorizationPermission permission)
        {
            var role = await roleManager.FindByIdAsync(permission.RoleId);
            var claims = await roleManager.GetClaimsAsync(role);
            var claim = claims.FirstOrDefault(c => c.Type == "Permission." + permission.ClaimType);
            if (claim == null)
                throw new KeyNotFoundException("Claim not found.");

            var readPermissions = JsonSerializer.Deserialize<AuthorizationPermission>(claim.Value);
            readPermissions.Read = permission.Read;
            readPermissions.Create = permission.Create;
            readPermissions.Update = permission.Update;
            readPermissions.Delete = permission.Delete;
            var updatedClaimValue = JsonSerializer.Serialize(readPermissions);
            await roleManager.RemoveClaimAsync(role, claim);
            await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim(claim.Type, updatedClaimValue));
            return true;
        }
       
    }
}

