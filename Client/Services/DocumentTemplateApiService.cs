using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Radzen;
using Vanigam.CRM.Client;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Helpers;
using Vanigam.CRM.Objects.Enums;

namespace Vanigam.CRM.Client;

public class DocumentTemplateApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<DocumentTemplate>(navigationManager, httpClient, authenticationStateProvider, configuration,
        "DocumentTemplates")
{
    public async Task<ODataServiceResult<DocxTemplate>> GetDocxTemplates(string dbSet,  string filter = default,
        string orderby = default,
        string expand = default, int? top = default, int? skip = default, bool? count = default,
        string format = default, string select = default)
    {
        filter = $"{nameof(DocumentTemplate.DbSet)} eq '{dbSet}' AND ({nameof(DocumentTemplate.TemplateType)} eq Vanigam.CRM.Objects.Entities.TemplateTypes'{TemplateTypes.DocxTemplate.ToString()}')";
        var uri = new Uri(BaseUri, "DocumentTemplates");
        uri = uri.GetODataUri(filter, top, skip, orderby, expand, select, count: count);

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await HttpClient.SendAsync(httpRequestMessage);

        return await VanigamAccountingHttpResponseMessageExtensions.ReadAsync<ODataServiceResult<DocxTemplate>>(response);
    }
    public async Task<ODataServiceResult<DocxMacroTemplate>> GetDocxMacroTemplates(string dbSet, string filter = default,
        string orderby = default,
        string expand = default, int? top = default, int? skip = default, bool? count = default,
        string format = default, string select = default)
    {
        filter = $"{nameof(DocumentTemplate.DbSet)} eq '{dbSet}' AND ({nameof(DocumentTemplate.TemplateType)} eq Vanigam.CRM.Objects.Entities.TemplateTypes'{TemplateTypes.DocxMacroTemplate.ToString()}')";
        var uri = new Uri(BaseUri, "DocumentTemplates");
        uri = uri.GetODataUri(filter, top, skip, orderby, expand, select, count: count);

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await HttpClient.SendAsync(httpRequestMessage);

        return await VanigamAccountingHttpResponseMessageExtensions.ReadAsync<ODataServiceResult<DocxMacroTemplate>>(response);
    }
}
