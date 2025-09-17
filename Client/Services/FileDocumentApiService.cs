using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Radzen;
using Vanigam.CRM.Client;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Helpers;

namespace Vanigam.CRM.Client;

public class FileDocumentApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<FileDocument>(navigationManager, httpClient, authenticationStateProvider, configuration,
        "FileDocuments")
{
    public async Task<ODataServiceResult<FileDocument>> GetFileDocumentsByPatient(string filter = default, string orderby = default,
       string expand = default, int? top = default, int? skip = default, bool? count = default,
       string format = default, string select = default, Guid? patientId = null)
    {
        if (patientId != null)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                filter += " and ";
            }
            Console.WriteLine(filter);
        }
        return await Get(filter, orderby, expand, top, skip, count, format, select);
    }

    public async Task<FileDocument> GetFileContent(Guid? oid)
    {
        var uri = new Uri(BaseUri, $"FileDocuments/GetFileContent(oid={oid})");
        //uri = uri.GetODataUri(null, null, null, null, null);
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await HttpClient.SendAsync(httpRequestMessage);
        var result= await VanigamAccountingHttpResponseMessageExtensions.ReadAsync<ODataServiceResult<FileDocument>>(response);
        return result?.Value?.FirstOrDefault();
    }
}
