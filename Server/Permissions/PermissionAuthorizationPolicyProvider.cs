using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Vanigam.CRM.Server.Permissions
{
    public class PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : DefaultAuthorizationPolicyProvider(options)
    {
        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Handle permission-based policies (e.g., "Permission.Customers")
            if (policyName.StartsWith("Permission.", StringComparison.OrdinalIgnoreCase))
            {
                var requirement = new PermissionRequirement(policyName);
                return new AuthorizationPolicyBuilder()
                    .AddRequirements(requirement).Build();
            }

            // Handle legacy "Is" policies
            if (policyName.StartsWith("Is", StringComparison.OrdinalIgnoreCase))
            {
                var requirement = new PermissionRequirement(policyName);
                return new AuthorizationPolicyBuilder()
                    .AddRequirements(requirement).Build();
            }

            return await base.GetPolicyAsync(policyName);
        }
    }
}

