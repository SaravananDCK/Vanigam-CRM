using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.DTOs;
using System.Net.Http.Json;

namespace Vanigam.CRM.Client;

public class OpportunityApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<Opportunity>(navigationManager, httpClient, authenticationStateProvider, configuration, nameof(VanigamAccountingDbContext.Opportunities))
{
    /// <summary>
    /// Gets status summary with counts for each OpportunityStage in a single API call
    /// </summary>
    /// <param name="request">Summary request with optional filters</param>
    /// <returns>Status summary response with counts</returns>
    public async Task<StatusSummaryResponse<OpportunityStage>> GetStatusSummaryAsync(StatusSummaryRequest request)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync(
                $"odata/VanigamAccountingService/Opportunities/status-summary",
                request);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<StatusSummaryResponse<OpportunityStage>>(json, GetJsonSerializerOptions())
                   ?? new StatusSummaryResponse<OpportunityStage>();
        }
        catch (Exception ex)
        {
            // Log error and return empty response as fallback
            Console.WriteLine($"Error getting opportunity status summary: {ex.Message}");
            return new StatusSummaryResponse<OpportunityStage>
            {
                TotalCount = 0,
                StatusCounts = new Dictionary<OpportunityStage, int>()
            };
        }
    }
}