using Microsoft.AspNetCore.Components;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Text;
using Vanigam.CRM.Client;
using Vanigam.CRM.Objects.DTOs;

namespace Vanigam.CRM.Client;

public class ReportApiService 
{
    protected readonly Uri BaseOdataUri;
    protected readonly Uri BaseUri;
    protected readonly HttpClient HttpClient;
    protected readonly ApplicationAuthenticationStateProvider AuthenticationStateProvider;
    protected readonly NavigationManager NavigationManager;
    private string _serializedText;
    public ReportApiService(NavigationManager navigationManager, HttpClient httpClient, IConfiguration configuration, AuthenticationStateProvider applicationAuthenticationStateProvider) 
    {
        this.HttpClient = httpClient;
        AuthenticationStateProvider = applicationAuthenticationStateProvider as ApplicationAuthenticationStateProvider;
        this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthenticationStateProvider.GetBearerToken());
        this.NavigationManager = navigationManager;
        BaseOdataUri = new Uri($"{navigationManager.BaseUri}odata/VanigamAccountingService/");
        BaseUri = new Uri($"{navigationManager.BaseUri}");
    }

    public async Task<bool> SaveReport(ReportParameterDto reportParameter)
    {
        var uri = new Uri(BaseUri, "Reports/SaveReport");
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
        _serializedText = JsonSerializer.Serialize(reportParameter);
        httpRequestMessage.Content = new StringContent(_serializedText, Encoding.UTF8, "application/json");
        var response = await HttpClient.SendAsync(httpRequestMessage);
        return response.IsSuccessStatusCode;
    }
}
