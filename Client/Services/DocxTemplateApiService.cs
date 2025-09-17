using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Vanigam.CRM.Client;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Services
{
    public class DocxTemplateApiService(
        NavigationManager navigationManager,
        HttpClient httpClient,
        AuthenticationStateProvider authenticationStateProvider,
        IConfiguration configuration)
        : BaseApiService<DocxTemplate>(navigationManager, httpClient, authenticationStateProvider, configuration,
            "DocxTemplates");
}

