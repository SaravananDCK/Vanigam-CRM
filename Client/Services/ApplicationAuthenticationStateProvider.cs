using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using DevExpress.Data.Mask.Internal;
using Vanigam.CRM.Objects;

namespace Vanigam.CRM.Client
{
    public class ApplicationAuthenticationStateProvider(SecurityService securityService) : AuthenticationStateProvider
    {
        private ApplicationAuthenticationState authenticationState;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();

            try
            {
                var state = await GetApplicationAuthenticationStateAsync();

                if (state.IsAuthenticated)
                {
                    identity = new ClaimsIdentity(state.Claims.Select(c => new Claim(c.Type, c.Value)), "Vanigam.CRM.Server");
                }
            }
            catch (HttpRequestException ex)
            {
            }

            var result = new AuthenticationState(new ClaimsPrincipal(identity));

            await securityService.InitializeAsync(result);

            return result;
        }

        public string GetBearerToken()
        {
            return authenticationState?.GetBearerToken();
        }
        private async Task<ApplicationAuthenticationState> GetApplicationAuthenticationStateAsync()
        {
            if (authenticationState == null)
            {
                authenticationState = await securityService.GetAuthenticationStateAsync();
            }

            return authenticationState;
        }
    }
}
