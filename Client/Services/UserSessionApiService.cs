using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using System.Net.Http.Json;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Helpers;

namespace Vanigam.CRM.Client.Services;

public class UserSessionApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<UserSession>(navigationManager, httpClient, authenticationStateProvider, configuration, "UserSession")
{
    public async Task<ODataServiceResult<UserSession>> GetActiveSessionsAsync()
    {
        try
        {
            
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri($"{navigationManager.BaseUri}odata/VanigamAccountingService/UserSessions/GetActiveSessions"));
            var response = await HttpClient.SendAsync(httpRequestMessage);

            return await VanigamAccountingHttpResponseMessageExtensions.ReadAsync<Radzen.ODataServiceResult<UserSession>>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading active sessions: {ex.Message}");
            return new ODataServiceResult<UserSession>();
        }
    }
   
    public async Task<bool> ForceLogoutAsync(Guid sessionId)
    {
        try
        {
            var response = await HttpClient.PostAsync($"odata/VanigamAccountingService/UserSessions/ForceLogout?sessionId={sessionId}", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error forcing logout: {ex.Message}");
            return false;
        }
    }

    public async Task<ODataServiceResult<UserSession>> GetUserSessionsByUserId(string filter = default, string orderby = default,
        string expand = default, int? top = default, int? skip = default, bool? count = default,
        string format = default, string select = default, string userId = null)
    {
        if (!string.IsNullOrEmpty(userId))
        {
            if (!string.IsNullOrEmpty(filter))
            {
                filter += " and ";
            }
            filter += $"{nameof(UserSession.UserId)} eq '{userId}'";
        }
        return await Get(filter, orderby, expand, top, skip, count, format, select);
    }

    public async Task<UserActivityAnalytics> GetUserActivityAnalyticsAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync<UserActivityAnalytics>(
                $"odata/MeditalkAIService/UserSessions/GetUserActivityAnalytics(fromDate={fromDate:yyyy-MM-dd},toDate={toDate:yyyy-MM-dd})");
            return response ?? new UserActivityAnalytics();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading user activity analytics: {ex.Message}");
            return new UserActivityAnalytics();
        }
    }

    public async Task<ODataServiceResult<UserActivitySummary>> GetUserActivitySummaryAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync< ODataServiceResult<UserActivitySummary>>(
                $"odata/MeditalkAIService/UserSessions/GetUserActivitySummary(fromDate={fromDate:yyyy-MM-dd},toDate={toDate:yyyy-MM-dd})");
            return response ?? new ODataServiceResult<UserActivitySummary>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading user activity summary: {ex.Message}");
            return new ODataServiceResult<UserActivitySummary>();
        }
    }

    public class ActiveSessionDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime LastActivityTime { get; set; }
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public string? Location { get; set; }
        public int SessionDurationMinutes { get; set; }
    }

    public class UserActivityAnalytics
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public double AverageSessionDurationHours { get; set; }
        public int TotalSessions { get; set; }
        public double TotalHoursLogged { get; set; }
        public List<DailyActivitySummary> DailyActivity { get; set; } = new();
    }

    public class DailyActivitySummary
    {
        public DateTime Date { get; set; }
        public int UniqueUsers { get; set; }
        public int TotalSessions { get; set; }
        public double TotalHours { get; set; }
        public double AverageSessionDuration { get; set; }
    }

    public class UserActivitySummary
    {
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public int TotalSessions { get; set; }
        public double TotalHoursLogged { get; set; }
        public double AverageSessionDurationHours { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime? FirstLoginTime { get; set; }
        public int DaysActive { get; set; }
        public string? MostUsedDevice { get; set; }
        public string? MostUsedLocation { get; set; }
    }
}
