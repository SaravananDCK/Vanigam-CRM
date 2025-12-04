using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Vanigam.CRM.Client;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Services
{
    public class DocxMacroTemplateApiService(
        NavigationManager navigationManager,
        HttpClient httpClient,
        AuthenticationStateProvider authenticationStateProvider,
        IConfiguration configuration)
        : BaseApiService<DocxMacroTemplate>(navigationManager, httpClient, authenticationStateProvider, configuration,
            nameof(VanigamAccountingDbContext.DocxMacroTemplates));
    
}

