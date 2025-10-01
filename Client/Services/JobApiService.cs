using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.DTOs;

namespace Vanigam.CRM.Client;

public class JobApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<Job>(navigationManager, httpClient, authenticationStateProvider, configuration, nameof(VanigamAccountingDbContext.Jobs))
{
    public async Task<StatusSummaryResponse<JobStatus>> GetStatusSummaryAsync(StatusSummaryRequest request)
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync(
                $"odata/VanigamAccountingService/Jobs/status-summary",
                request);

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<StatusSummaryResponse<JobStatus>>(json, GetJsonSerializerOptions())
                   ?? new StatusSummaryResponse<JobStatus>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting job status summary: {ex.Message}");
            return new StatusSummaryResponse<JobStatus>
            {
                TotalCount = 0,
                StatusCounts = new Dictionary<JobStatus, int>()
            };
        }
    }
}