using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Vanigam.CRM.Server.Permissions;

public class CustomUserClaimsPrincipalFactory<TUser, TRole>(
    UserManager<TUser> userManager,
    RoleManager<TRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<TUser, TRole>(userManager, roleManager, optionsAccessor)
    where TUser : IdentityUser<Guid>
    where TRole : IdentityRole<Guid>
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TUser user)
    {
        // Generate user claims as usual
        var identity = await base.GenerateClaimsAsync(user);

        // Remove role claims from the generated claims
        var roleClaims = identity.Claims
            .Where(c => c.Type.StartsWith("Permission."))
            .ToList();

        foreach (var claim in roleClaims)
        {
            identity.RemoveClaim(claim);
        }

        return identity;
    }
}
