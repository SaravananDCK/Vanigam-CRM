using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Text.Json;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Services;

public class LeadConversionApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
{
    private readonly string _baseUrl = $"{configuration["ApiUrl"] ?? navigationManager.BaseUri.TrimEnd('/')}/api/leadconversion";

    public async Task<Opportunity?> ConvertLeadToOpportunityAsync(Guid leadId, string opportunityTitle, decimal estimatedValue, DateTime expectedCloseDate)
    {
        try
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            if (!authState.User.Identity?.IsAuthenticated == true)
            {
                navigationManager.NavigateTo("/login");
                return null;
            }

            var request = new
            {
                LeadId = leadId,
                OpportunityTitle = opportunityTitle,
                EstimatedValue = estimatedValue,
                ExpectedCloseDate = expectedCloseDate
            };

            var response = await httpClient.PostAsJsonAsync($"{_baseUrl}/lead-to-opportunity", request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Opportunity>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                throw new InvalidOperationException(errorResponse?.Error ?? "Invalid operation");
            }
            else
            {
                response.EnsureSuccessStatusCode();
                return null;
            }
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            throw new HttpRequestException($"Error converting lead to opportunity: {ex.Message}", ex);
        }
    }

    public async Task<Customer?> ConvertOpportunityToCustomerAsync(Guid opportunityId)
    {
        try
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            if (!authState.User.Identity?.IsAuthenticated == true)
            {
                navigationManager.NavigateTo("/login");
                return null;
            }

            var request = new
            {
                OpportunityId = opportunityId
            };

            var response = await httpClient.PostAsJsonAsync($"{_baseUrl}/opportunity-to-customer", request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Customer>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                throw new InvalidOperationException(errorResponse?.Error ?? "Invalid operation");
            }
            else
            {
                response.EnsureSuccessStatusCode();
                return null;
            }
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            throw new HttpRequestException($"Error converting opportunity to customer: {ex.Message}", ex);
        }
    }

    private class ErrorResponse
    {
        public string? Error { get; set; }
    }
}