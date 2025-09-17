using System.Net;
using Vanigam.CRM.Client.Components;
using Microsoft.AspNetCore.Components;

namespace Vanigam.CRM.Client.Handlers
{
    public class VanigamAccountingContextDelegatingHandler(ILocalStorageService localStorage, NavigationManager navigationManager)
        : DelegatingHandler
    {
        public const string SelectedHomeHealthRegion = nameof(SelectedHomeHealthRegion);
        private ILocalStorageService LocalStorageService { get; set; } = localStorage;
        public NavigationManager NavigationManager { get; } = navigationManager;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string? homeHealthRegionId = await LocalStorageService.GetItemAsync(SelectedHomeHealthRegion);
            if (!string.IsNullOrEmpty(homeHealthRegionId) && homeHealthRegionId != "null")
            {
                request.Headers.Add(SelectedHomeHealthRegion, homeHealthRegionId);
            }
            var response = await base.SendAsync(request, cancellationToken);
            //if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                NavigationManager.NavigateTo(@"\Login?sessionInvalid=true", forceLoad: true);
            }
            return response;
        }
    }
}

