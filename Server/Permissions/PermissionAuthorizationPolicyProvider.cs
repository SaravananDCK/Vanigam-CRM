using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Vanigam.CRM.Server.Permissions
{
    public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionAuthorizationPolicyProvider(
            IOptions<AuthorizationOptions> options) : base(options) { }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (!policyName.StartsWith("Is", StringComparison.OrdinalIgnoreCase))
                return await base.GetPolicyAsync(policyName);

            // Here we create the instance of our requirement
            var requirement = new PermissionRequirement(policyName);

            // Now we use the builder to create a policy, adding our requirement
            return new AuthorizationPolicyBuilder()
                .AddRequirements(requirement).Build();
        }
    }
}

