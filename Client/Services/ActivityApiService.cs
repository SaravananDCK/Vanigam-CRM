using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.DTOs;
using System.Net.Http.Json;

namespace Vanigam.CRM.Client;

public class ActivityApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<Activity>(navigationManager, httpClient, authenticationStateProvider, configuration, nameof(VanigamAccountingDbContext.Activities))
{
    /// <summary>
    /// Gets status summary with counts for each ActivityStatus in a single API call
    /// </summary>
    /// <param name="request">Summary request with optional filters</param>
    /// <returns>Status summary response with counts</returns>
    public async Task<StatusSummaryResponse<ActivityStatus>> GetStatusSummaryAsync(StatusSummaryRequest request)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync(
                $"odata/VanigamAccountingService/Activities/status-summary",
                request);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<StatusSummaryResponse<ActivityStatus>>(json, GetJsonSerializerOptions())
                   ?? new StatusSummaryResponse<ActivityStatus>();
        }
        catch (Exception ex)
        {
            // Log error and return empty response as fallback
            Console.WriteLine($"Error getting activity status summary: {ex.Message}");
            return new StatusSummaryResponse<ActivityStatus>
            {
                TotalCount = 0,
                StatusCounts = new Dictionary<ActivityStatus, int>()
            };
        }
    }
}