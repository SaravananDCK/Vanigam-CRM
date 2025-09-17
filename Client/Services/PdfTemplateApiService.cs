using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using Vanigam.CRM.Client;
using Vanigam.CRM.Objects.Entities;
namespace Vanigam.CRM.AI.Client;

public class PdfTemplateApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<PdfTemplate>(navigationManager, httpClient, authenticationStateProvider, configuration,
        "PdfTemplates")
{
    public async Task<PdfTemplate?> GetPdfTemplate(string expand = default, Guid templateId = default)
    {
        var uri = new Uri(BaseUri, $"PdfTemplates({templateId})");

        uri = uri.GetODataUri(null, null, null, null, expand);

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await HttpClient.SendAsync(httpRequestMessage);
        return await response.ReadAsync<PdfTemplate>();
    }
}
