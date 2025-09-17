using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Radzen;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Vanigam.CRM.Objects.Contracts;
using Vanigam.CRM.Objects.Helpers;

namespace Vanigam.CRM.Client;

public abstract class BaseApiService<T> where T : BaseClass
{
    public string ControllerName { get; }
    protected readonly HttpClient HttpClient;
    protected readonly Uri BaseUri;
    protected readonly Uri BaseIdentityUri;
    protected readonly ApplicationAuthenticationStateProvider AuthenticationStateProvider;
    protected readonly NavigationManager NavigationManager;
    public string BearerToken { get; private set; }
    protected BaseApiService(NavigationManager navigationManager, HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, IConfiguration configuration, string controllerName)
    {
        ControllerName = controllerName;
        this.HttpClient = httpClient;
        AuthenticationStateProvider = authenticationStateProvider as ApplicationAuthenticationStateProvider;
        BearerToken = AuthenticationStateProvider.GetBearerToken();
        this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);


        this.NavigationManager = navigationManager;
        this.BaseUri = new Uri($"{navigationManager.BaseUri}odata/VanigamAccountingService/");
        this.BaseIdentityUri = new Uri($"{navigationManager.BaseUri}odata/Identity/");
    }
    public async System.Threading.Tasks.Task ExportToCSV(Query query = null, string fileName = null)
    {
        NavigationManager.NavigateTo(query != null ? query.ToUrl($"export/VanigamAccountingService/{ControllerName}/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/VanigamAccountingAIService/{ControllerName}/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
    }

    void OnGet(HttpRequestMessage requestMessage) { }

    public async Task<Radzen.ODataServiceResult<T>> Get(Query query)
    {
        return await Get(filter: $"{query.Filter}", orderBy: $"{query.OrderBy}", top: query.Top, skip: query.Skip, count: query.Top != null && query.Skip != null);
    }

    public async Task<Radzen.ODataServiceResult<T>> Get(string filter = default(string), string orderBy = default(string), string expand = default(string), int? top = default(int?), int? skip = default(int?), bool? count = default(bool?), string format = default(string), string select = default(string), string apply = default(string), bool expandUserInfo=false)
    {
        var uri = new Uri(BaseUri, $"{ControllerName}");
        if (orderBy!=null && orderBy.StartsWith("np("))
        {
            orderBy = String.Empty;
        }
        uri = uri.GetODataUri(filter: filter, top: top, skip: skip, orderby: orderBy, expand: expand, select: select, count: count,apply: apply);
        if (expandUserInfo)
        {
            UriBuilder uriBuilder = new UriBuilder(uri)
            {
                Query = uri.Query
            };
            uriBuilder.Query += "&expandUserInfo=true";
            uri = uriBuilder.Uri;
        }
        
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

        OnGet(httpRequestMessage);
        var response = await HttpClient.SendAsync(httpRequestMessage);

        return await VanigamAccountingHttpResponseMessageExtensions.ReadAsync<Radzen.ODataServiceResult<T>>(response);
    }

    void OnCreate(HttpRequestMessage requestMessage) { }

    public async Task<T> Create(T entity = default(T), bool skipAuditLog = false)
    {
        var uri = new Uri(BaseUri, $"{ControllerName}");

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
        httpRequestMessage.Headers.Add("SkipAuditLog", skipAuditLog.ToString());
        var serializedText = ODataJsonSerializer.Serialize(entity, GetJsonSerializerOptions());
        httpRequestMessage.Content = new StringContent(serializedText, Encoding.UTF8, "application/json");

        OnCreate(httpRequestMessage);

        var response = await HttpClient.SendAsync(httpRequestMessage);

        return await response.ReadAsync<T>();
    }

    protected JsonSerializerOptions GetJsonSerializerOptions()
    {
        var jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerOptions.Default);
        //jsonSerializerOptions.Converters.Add(new MeditalkDateTimeConverter());
        //jsonSerializerOptions.Converters.Add(new MeditalkNullableDateTimeConverter());
        return jsonSerializerOptions;
    }
    void OnDelete(HttpRequestMessage requestMessage) { }

    public async Task<HttpResponseMessage> Delete(Guid oid = default(Guid))
    {
        var uri = new Uri(BaseUri, $"{ControllerName}({oid})");
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);
        OnDelete(httpRequestMessage);
        var result = await HttpClient.SendAsync(httpRequestMessage);
        if (result.IsSuccessStatusCode==false)
        {
            throw new Exception($"Cant delete {typeof(T).Name}");
        }
        return result;
    }

    void OnGetByOid(HttpRequestMessage requestMessage) { }

    public async Task<T> GetByOid(string expand = default(string), Guid oid = default(Guid), string select = null)
    {
        var uri = new Uri(BaseUri, $"{ControllerName}({oid})");

        uri = uri.GetODataUri(filter: null, top: null, skip: null, orderby: null, expand: expand, select: select, count: null);

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

        OnGetByOid(httpRequestMessage);

        var response = await HttpClient.SendAsync(httpRequestMessage);

        return await response.ReadAsync<T>();
    }

    void OnUpdate(HttpRequestMessage requestMessage) { }

    public async Task<HttpResponseMessage> Update(Guid oid = default(Guid), T entity = default(T),bool skipAuditLog=false)
    {
        try
        {
            var uri = new Uri(BaseUri, $"{ControllerName}({oid})");

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Patch, uri);

            httpRequestMessage.Headers.Add("If-Match", entity.ETag);
            httpRequestMessage.Headers.Add("SkipAuditLog", skipAuditLog.ToString());

            var serializedText = ODataJsonSerializer.Serialize(entity, GetJsonSerializerOptions());
            httpRequestMessage.Content = new StringContent(serializedText, Encoding.UTF8, "application/json");

            OnUpdate(httpRequestMessage);

            var response= await HttpClient.SendAsync(httpRequestMessage); ;
            response.EnsureSuccessStatusCode();
            return response;
        }     
        catch(HttpRequestException exception)
        {
            throw exception; 
        }
        catch (Exception e)
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }
        
    }

}
