using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Objects.Helpers
{
    public static class ClaimPrincipalHelper
    {
        public static string GetUserId(this System.Security.Claims.ClaimsPrincipal principal)
        {
            return principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        public static string GetUserName(this System.Security.Claims.ClaimsPrincipal principal)
        {
            return principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        }

        public static string GetUserEmail(this System.Security.Claims.ClaimsPrincipal principal)
        {
            return principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        }

        public static string GetUserRole(this System.Security.Claims.ClaimsPrincipal principal)
        {
            return principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        }

        public static string GetUserFullName(this System.Security.Claims.ClaimsPrincipal principal)
        {
            return principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value + " " + principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value;
        }
    }
}

