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

    public async Task<Job?> BulkSaveJobWithMaterialsAsync(JobBulkSaveDTO jobData)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity?.IsAuthenticated == true)
        {
            navigationManager.NavigateTo("/authentication/login");
            return null;
        }

        try
        {
            var json = JsonSerializer.Serialize(jobData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"odata/VanigamAccountingService/jobs/bulk-save", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Job>(responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to save job: {errorMessage}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Network error: {ex.Message}");
        }
    }

    public async Task<List<MaterialUsageDTO>> GetMaterialsForEditingAsync(Guid jobId)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity?.IsAuthenticated == true)
        {
            navigationManager.NavigateTo("/authentication/login");
            return new List<MaterialUsageDTO>();
        }

        try
        {
            var response = await httpClient.GetAsync($"api/job/{jobId}/materials-for-editing");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<MaterialUsageDTO>>(responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new List<MaterialUsageDTO>();
            }
            else
            {
                throw new InvalidOperationException($"Failed to load materials: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Network error: {ex.Message}");
        }
    }
}