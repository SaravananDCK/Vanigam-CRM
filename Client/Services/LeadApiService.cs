using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.DTOs;
using System.Net.Http.Json;

namespace Vanigam.CRM.Client;

public class LeadApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<Lead>(navigationManager, httpClient, authenticationStateProvider, configuration, nameof(VanigamAccountingDbContext.Leads))
{
    /// <summary>
    /// Gets status summary with counts for each LeadStatus in a single API call
    /// </summary>
    /// <param name="request">Summary request with optional filters</param>
    /// <returns>Status summary response with counts</returns>
    public async Task<StatusSummaryResponse<LeadStatus>> GetStatusSummaryAsync(StatusSummaryRequest request)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync(
                $"odata/VanigamAccountingService/Leads/status-summary",
                request);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<StatusSummaryResponse<LeadStatus>>(json, GetJsonSerializerOptions())
                   ?? new StatusSummaryResponse<LeadStatus>();
        }
        catch (Exception ex)
        {
            // Log error and return empty response as fallback
            Console.WriteLine($"Error getting lead status summary: {ex.Message}");
            return new StatusSummaryResponse<LeadStatus>
            {
                TotalCount = 0,
                StatusCounts = new Dictionary<LeadStatus, int>()
            };
        }
    }

    /// <summary>
    /// Converts a Lead to an Opportunity
    /// </summary>
    /// <param name="leadId">ID of the Lead to convert</param>
    /// <param name="opportunityTitle">Title for the new Opportunity</param>
    /// <param name="estimatedValue">Estimated value for the Opportunity</param>
    /// <param name="expectedCloseDate">Expected close date for the Opportunity (must be UTC)</param>
    /// <returns>The created Opportunity</returns>
    public async Task<Opportunity?> ConvertLeadToOpportunityAsync(Guid leadId, string opportunityTitle, decimal estimatedValue, DateTimeOffset expectedCloseDate)
    {
        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            if (!authState.User.Identity?.IsAuthenticated == true)
            {
                NavigationManager.NavigateTo("/login");
                return null;
            }

            var request = new ConvertLeadToOpportunityRequest
            {
                LeadId = leadId,
                OpportunityTitle = opportunityTitle,
                EstimatedValue = estimatedValue,
                ExpectedCloseDate = expectedCloseDate
            };

            var response = await HttpClient.PostAsJsonAsync(
                $"odata/VanigamAccountingService/Leads/convert-to-opportunity",
                request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Opportunity>(json, GetJsonSerializerOptions());
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, GetJsonSerializerOptions());
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

    /// <summary>
    /// Converts an Opportunity to a Customer
    /// </summary>
    /// <param name="opportunityId">ID of the Opportunity to convert</param>
    /// <returns>The created Customer</returns>
    public async Task<Customer?> ConvertOpportunityToCustomerAsync(Guid opportunityId)
    {
        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            if (!authState.User.Identity?.IsAuthenticated == true)
            {
                NavigationManager.NavigateTo("/login");
                return null;
            }

            var request = new ConvertOpportunityToCustomerRequest
            {
                OpportunityId = opportunityId
            };

            var response = await HttpClient.PostAsJsonAsync(
                $"odata/VanigamAccountingService/Leads/convert-to-customer",
                request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Customer>(json, GetJsonSerializerOptions());
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, GetJsonSerializerOptions());
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

    /// <summary>
    /// Error response model for API error handling
    /// </summary>
    private class ErrorResponse
    {
        public string? Error { get; set; }
    }
}