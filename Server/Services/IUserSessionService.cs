using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;

namespace Meditalk.AI.Server.Services
{
    public interface IUserSessionService
    {
        Task<UserSession> RecordLoginAsync(string userId, string? userName, int? tenantId,
            string? deviceInfo, string? ipAddress, string? userAgent,
            string? location = null, decimal? latitude = null, decimal? longitude = null);

        Task RecordLogoutAsync(string userId, string? sessionId = null);

        Task UpdateLastActivityAsync(string userId);

        Task<List<UserSession>> GetActiveSessionsAsync(int? tenantId = null);

        Task<UserSession?> GetUserActiveSessionAsync(string userId);

        Task<List<UserSession>> GetUserSessionHistoryAsync(string userId, int days = 30);

        Task ForceLogoutAsync(Guid sessionId);

        Task CleanupExpiredSessionsAsync(int timeoutMinutes = 30);

        Task<int> GetActiveUserCountAsync(int? tenantId = null);

        Task<Dictionary<string, object>> GetSessionAnalyticsAsync(int? tenantId = null, int days = 7);

        Task<UserActivityAnalytics> GetUserActivityAnalyticsAsync(int? tenantId, DateTimeOffset fromDate, DateTimeOffset toDate);

        Task<IEnumerable<UserActivitySummary>> GetUserActivitySummaryAsync(int? tenantId, DateTimeOffset fromDate, DateTimeOffset toDate);
    }
}
