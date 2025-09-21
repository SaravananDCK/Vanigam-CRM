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
}