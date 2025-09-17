using System.Text.Json;
using StackExchange.Redis;
using Vanigam.CRM.Objects.Redis;

namespace Vanigam.CRM.Server.Services
{
    public interface IRedisSessionService
    {
        Task<string> CreateSessionAsync(string userId, string? userName, int? tenantId, 
            string? deviceInfo, string? ipAddress, string? userAgent, 
            string? location = null, decimal? latitude = null, decimal? longitude = null);
        
        Task UpdateSessionActivityAsync(string sessionId);
        Task UpdateSessionActivityByUserIdAsync(string userId);
        Task EndSessionAsync(string sessionId);
        Task EndUserSessionsAsync(string userId);
        Task<List<ActiveSessionInfo>> GetActiveSessionsAsync(int? tenantId = null);
        Task<ActiveSessionInfo?> GetSessionAsync(string sessionId);
        Task<List<ActiveSessionInfo>> GetUserActiveSessionsAsync(string userId);
        Task<int> GetActiveUserCountAsync(int? tenantId = null);
        Task CleanupExpiredSessionsAsync(int timeoutMinutes = 30);
        Task<Dictionary<string, object>> GetSessionAnalyticsAsync(int? tenantId = null);
    }

    public class RedisSessionService : IRedisSessionService
    {
        private readonly RedisService _redisService;
        private readonly ILogger<RedisSessionService> _logger;
        private readonly IDatabase _database;

        // Redis key patterns
        private const string SESSION_KEY_PREFIX = "session:";
        private const string USER_SESSIONS_KEY_PREFIX = "user_sessions:";
        private const string TENANT_SESSIONS_KEY_PREFIX = "tenant_sessions:";
        private const string ACTIVE_SESSIONS_SET = "active_sessions";

        public RedisSessionService(RedisService redisService, ILogger<RedisSessionService> logger)
        {
            _redisService = redisService;
            _logger = logger;
            _database = redisService.Database;
        }

        public async Task<string> CreateSessionAsync(string userId, string? userName, int? tenantId,
            string? deviceInfo, string? ipAddress, string? userAgent,
            string? location = null, decimal? latitude = null, decimal? longitude = null)
        {
            try
            {
                var sessionId = Guid.NewGuid().ToString();
                var now = DateTime.UtcNow;

                var sessionInfo = new ActiveSessionInfo
                {
                    SessionId = sessionId,
                    UserId = userId,
                    UserName = userName,
                    TenantId = tenantId,
                    LoginTime = now,
                    LastActivityTime = now,
                    DeviceInfo = deviceInfo,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Location = location,
                    Latitude = latitude,
                    Longitude = longitude
                };

                var sessionJson = JsonSerializer.Serialize(sessionInfo);

                // Store session data
                await _database.StringSetAsync($"{SESSION_KEY_PREFIX}{sessionId}", sessionJson, TimeSpan.FromHours(24));

                // Add to user's active sessions
                await _database.SetAddAsync($"{USER_SESSIONS_KEY_PREFIX}{userId}", sessionId);

                // Add to tenant's active sessions if tenant exists
                if (tenantId.HasValue)
                {
                    await _database.SetAddAsync($"{TENANT_SESSIONS_KEY_PREFIX}{tenantId}", sessionId);
                }

                // Add to global active sessions set
                await _database.SetAddAsync(ACTIVE_SESSIONS_SET, sessionId);

                _logger.LogInformation("Redis session created for user {UserId}: {SessionId}", userId, sessionId);
                
                // Debug: Check how many sessions this user now has
                var userSessionCount = await _database.SetLengthAsync($"{USER_SESSIONS_KEY_PREFIX}{userId}");
                _logger.LogDebug("User {UserId} now has {Count} active Redis sessions", userId, userSessionCount);
                
                return sessionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Redis session for user {UserId}", userId);
                throw;
            }
        }

        public async Task UpdateSessionActivityAsync(string sessionId)
        {
            try
            {
                var sessionKey = $"{SESSION_KEY_PREFIX}{sessionId}";
                var sessionJson = await _database.StringGetAsync(sessionKey);
                
                if (sessionJson.HasValue)
                {
                    var sessionInfo = JsonSerializer.Deserialize<ActiveSessionInfo>(sessionJson!);
                    if (sessionInfo != null)
                    {
                        sessionInfo.LastActivityTime = DateTime.UtcNow;
                        var updatedJson = JsonSerializer.Serialize(sessionInfo);
                        
                        await _database.StringSetAsync(sessionKey, updatedJson, TimeSpan.FromHours(24));
                        _logger.LogDebug("Updated activity for session {SessionId}", sessionId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error updating session activity for {SessionId}", sessionId);
            }
        }

        public async Task UpdateSessionActivityByUserIdAsync(string userId)
        {
            try
            {
                var userSessionsKey = $"{USER_SESSIONS_KEY_PREFIX}{userId}";
                var sessionIds = await _database.SetMembersAsync(userSessionsKey);

                foreach (var sessionId in sessionIds)
                {
                    await UpdateSessionActivityAsync(sessionId!);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error updating session activity for user {UserId}", userId);
            }
        }

        public async Task EndSessionAsync(string sessionId)
        {
            try
            {
                var sessionKey = $"{SESSION_KEY_PREFIX}{sessionId}";
                var sessionJson = await _database.StringGetAsync(sessionKey);
                
                if (sessionJson.HasValue)
                {
                    var sessionInfo = JsonSerializer.Deserialize<ActiveSessionInfo>(sessionJson!);
                    if (sessionInfo != null)
                    {
                        // Remove from user's active sessions
                        await _database.SetRemoveAsync($"{USER_SESSIONS_KEY_PREFIX}{sessionInfo.UserId}", sessionId);

                        // Remove from tenant's active sessions
                        if (sessionInfo.TenantId.HasValue)
                        {
                            await _database.SetRemoveAsync($"{TENANT_SESSIONS_KEY_PREFIX}{sessionInfo.TenantId}", sessionId);
                        }

                        // Remove from global active sessions
                        await _database.SetRemoveAsync(ACTIVE_SESSIONS_SET, sessionId);

                        // Delete session data
                        await _database.KeyDeleteAsync(sessionKey);

                        _logger.LogInformation("Ended Redis session {SessionId} for user {UserId}", sessionId, sessionInfo.UserId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending session {SessionId}", sessionId);
                throw;
            }
        }

        public async Task EndUserSessionsAsync(string userId)
        {
            try
            {
                var userSessionsKey = $"{USER_SESSIONS_KEY_PREFIX}{userId}";
                var sessionIds = await _database.SetMembersAsync(userSessionsKey);

                _logger.LogDebug("Found {Count} existing Redis sessions for user {UserId}", sessionIds.Length, userId);

                foreach (var sessionId in sessionIds)
                {
                    await EndSessionAsync(sessionId!);
                }

                _logger.LogInformation("Ended {Count} Redis sessions for user {UserId}", sessionIds.Length, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending sessions for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<ActiveSessionInfo>> GetActiveSessionsAsync(int? tenantId = null)
        {
            try
            {
                var sessionIds = tenantId.HasValue
                    ? await _database.SetMembersAsync($"{TENANT_SESSIONS_KEY_PREFIX}{tenantId}")
                    : await _database.SetMembersAsync(ACTIVE_SESSIONS_SET);

                var sessions = new List<ActiveSessionInfo>();

                foreach (var sessionId in sessionIds)
                {
                    var session = await GetSessionAsync(sessionId!);
                    if (session != null)
                    {
                        sessions.Add(session);
                    }
                }

                return sessions.OrderByDescending(s => s.LoginTime).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sessions for tenant {TenantId}", tenantId);
                return new List<ActiveSessionInfo>();
            }
        }

        public async Task<ActiveSessionInfo?> GetSessionAsync(string sessionId)
        {
            try
            {
                var sessionJson = await _database.StringGetAsync($"{SESSION_KEY_PREFIX}{sessionId}");
                return sessionJson.HasValue ? JsonSerializer.Deserialize<ActiveSessionInfo>(sessionJson!) : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting session {SessionId}", sessionId);
                return null;
            }
        }

        public async Task<List<ActiveSessionInfo>> GetUserActiveSessionsAsync(string userId)
        {
            try
            {
                var userSessionsKey = $"{USER_SESSIONS_KEY_PREFIX}{userId}";
                var sessionIds = await _database.SetMembersAsync(userSessionsKey);

                var sessions = new List<ActiveSessionInfo>();

                foreach (var sessionId in sessionIds)
                {
                    var session = await GetSessionAsync(sessionId!);
                    if (session != null)
                    {
                        sessions.Add(session);
                    }
                }

                return sessions.OrderByDescending(s => s.LoginTime).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sessions for user {UserId}", userId);
                return new List<ActiveSessionInfo>();
            }
        }

        public async Task<int> GetActiveUserCountAsync(int? tenantId = null)
        {
            try
            {
                var sessionIds = tenantId.HasValue
                    ? await _database.SetMembersAsync($"{TENANT_SESSIONS_KEY_PREFIX}{tenantId}")
                    : await _database.SetMembersAsync(ACTIVE_SESSIONS_SET);

                var uniqueUsers = new HashSet<string>();

                foreach (var sessionId in sessionIds)
                {
                    var session = await GetSessionAsync(sessionId!);
                    if (session != null)
                    {
                        uniqueUsers.Add(session.UserId);
                    }
                }

                return uniqueUsers.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active user count for tenant {TenantId}", tenantId);
                return 0;
            }
        }

        public async Task CleanupExpiredSessionsAsync(int timeoutMinutes = 30)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddMinutes(-timeoutMinutes);
                var sessionIds = await _database.SetMembersAsync(ACTIVE_SESSIONS_SET);
                var expiredCount = 0;

                foreach (var sessionId in sessionIds)
                {
                    var session = await GetSessionAsync(sessionId!);
                    if (session != null && session.LastActivityTime < cutoffTime)
                    {
                        await EndSessionAsync(sessionId!);
                        expiredCount++;
                    }
                }

                _logger.LogInformation("Cleaned up {Count} expired Redis sessions", expiredCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired Redis sessions");
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetSessionAnalyticsAsync(int? tenantId = null)
        {
            try
            {
                var sessions = await GetActiveSessionsAsync(tenantId);
                var now = DateTime.UtcNow;

                return new Dictionary<string, object>
                {
                    ["ActiveSessions"] = sessions.Count,
                    ["UniqueUsers"] = sessions.Select(s => s.UserId).Distinct().Count(),
                    ["AverageSessionMinutes"] = sessions.Any() 
                        ? sessions.Average(s => (now - s.LoginTime).TotalMinutes) 
                        : 0,
                    ["TopDevices"] = sessions.GroupBy(s => s.DeviceInfo)
                        .Where(g => !string.IsNullOrEmpty(g.Key))
                        .OrderByDescending(g => g.Count())
                        .Take(5)
                        .ToDictionary(g => g.Key!, g => g.Count()),
                    ["TopLocations"] = sessions.GroupBy(s => s.Location)
                        .Where(g => !string.IsNullOrEmpty(g.Key))
                        .OrderByDescending(g => g.Count())
                        .Take(5)
                        .ToDictionary(g => g.Key!, g => g.Count())
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session analytics for tenant {TenantId}", tenantId);
                return new Dictionary<string, object>();
            }
        }
    }

    public class ActiveSessionInfo
    {
        public string SessionId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public int? TenantId { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime LastActivityTime { get; set; }
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Location { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public int SessionDurationMinutes => (int)(DateTime.UtcNow - LoginTime).TotalMinutes;
    }
}
