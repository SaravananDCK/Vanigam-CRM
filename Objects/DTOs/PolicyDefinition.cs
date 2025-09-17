using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Vanigam.CRM.Objects;

namespace Vanigam.Objects.DTOs
{
    public class PolicyDefinition
    {
        public static readonly PolicyDefinition[] PolicyDefinitions = new[]
        {
        new PolicyDefinition(ApplicationPolicy.IsAdministrator, context => ApplicationRole.AdministratorRoles.Any(role => context.User.HasClaim(ClaimTypes.Role, role))),
        new PolicyDefinition(ApplicationPolicy.IsSuperUser, context => context.User.HasClaim(ClaimTypes.Role, ApplicationRole.SuperUserRole)),      
    }; 
        public string PolicyName { get; }
        public Func<AuthorizationHandlerContext, bool> Requirement { get; }

        public PolicyDefinition(string policyName, Func<AuthorizationHandlerContext, bool> requirement)
        {
            PolicyName = policyName;
            Requirement = requirement;
        }
    }
}

