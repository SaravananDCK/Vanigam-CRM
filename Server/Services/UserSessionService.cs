using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Server.Services;
using Vanigam.CRM.Objects.DTOs;

namespace Meditalk.AI.Server.Services
{
    public class UserSessionService : BaseService<UserSession>, IUserSessionService
    {
        private readonly VanigamAccountingDbContext _context;
        private readonly IRedisSessionService _redisSessionService;
        private readonly ILogger<BaseService<UserSession>> _logger;

        public UserSessionService(VanigamAccountingDbContext context, IRedisSessionService redisSessionService, ILogger<BaseService<UserSession>> logger)
            : base(context, logger)
        {
            _context = context;
            _redisSessionService = redisSessionService;
            _logger = logger;
        }

        public override DbSet<UserSession> GetDbSet()
        {
            return Context.UserSessions;
        }

        public async Task<UserSession> RecordLoginAsync(string userId, string? userName, int? tenantId,
            string? deviceInfo, string? ipAddress, string? userAgent,
            string? location = null, decimal? latitude = null, decimal? longitude = null)
        {
            try
            {
                // End any existing Redis sessions for this user
                await _redisSessionService.EndUserSessionsAsync(userId);

                // End any existing database sessions for this user
                var existingSessions = await _context.UserSessions
                    .Where(s => s.UserId == userId && s.IsActive)
                    .ToListAsync();

                foreach (var session in existingSessions)
                {
                    session.IsActive = false;
                    session.LogoutTime = DateTime.UtcNow;
                }

                // Create new Redis session for real-time tracking
                var sessionId = await _redisSessionService.CreateSessionAsync(userId, userName, tenantId,
                    deviceInfo, ipAddress, userAgent, location, latitude, longitude);

                // Create database record for historical tracking
                var newSession = new UserSession
                {
                    UserId = userId,
                    UserName = userName,
                    TenantId = tenantId,
                    LoginTime = DateTime.UtcNow,
                    LastActivityTime = DateTime.UtcNow,
                    IsActive = true,
                    DeviceInfo = deviceInfo,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Location = location,
                    Latitude = latitude,
                    Longitude = longitude
                };

                _context.UserSessions.Add(newSession);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Login recorded for user {UserId} from {IpAddress} (Redis: {SessionId})", userId, ipAddress, sessionId);
                return newSession;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording login for user {UserId}", userId);
                throw;
            }
        }

        public async Task RecordLogoutAsync(string userId, string? sessionId = null)
        {
            try
            {
                // End Redis sessions (real-time)
                await _redisSessionService.EndUserSessionsAsync(userId);

                // Update database records
                var query = _context.UserSessions.Where(s => s.UserId == userId && s.IsActive);

                if (!string.IsNullOrEmpty(sessionId) && Guid.TryParse(sessionId, out var id))
                {
                    query = query.Where(s => s.Oid == id);
                }

                var sessions = await query.ToListAsync();

                foreach (var session in sessions)
                {
                    session.IsActive = false;
                    session.LogoutTime = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Logout recorded for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording logout for user {UserId}", userId);
                throw;
            }
        }

        public async Task UpdateLastActivityAsync(string userId)
        {
            try
            {
                // Update Redis session (fast, real-time)
                await _redisSessionService.UpdateSessionActivityByUserIdAsync(userId);

                // Note: Database updates are handled by a background job to avoid context disposal issues
                // The Hangfire cleanup job will periodically sync Redis state to database
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error updating activity for user {UserId}", userId);
                // Don't throw - this is not critical
            }
        }



        public async Task<List<UserSession>> GetActiveSessionsAsync(int? tenantId = null)
        {
            try
            {
                // Get active sessions from Redis (real-time data) - this is the primary source
                var redisSessions = await _redisSessionService.GetActiveSessionsAsync(tenantId);

                // Convert Redis sessions to UserSession format for compatibility
                var userSessions = redisSessions.Select(rs => new UserSession
                {
                    Oid = Guid.Parse(rs.SessionId), // Use Redis session ID as Oid
                    UserId = rs.UserId,
                    UserName = rs.UserName,
                    TenantId = rs.TenantId,
                    LoginTime = rs.LoginTime,
                    LastActivityTime = rs.LastActivityTime,
                    IsActive = true,
                    DeviceInfo = rs.DeviceInfo,
                    IpAddress = rs.IpAddress,
                    UserAgent = rs.UserAgent,
                    Location = rs.Location,
                    Latitude = rs.Latitude,
                    Longitude = rs.Longitude
                }).ToList();

                _logger.LogDebug("Retrieved {Count} active sessions from Redis", userSessions.Count);
                return userSessions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sessions from Redis, falling back to database");

                // Fallback to database if Redis fails (but this should be rare)
                var query = _context.UserSessions
                    .Include(s => s.User)
                    .Where(s => s.IsActive);

                if (tenantId.HasValue)
                {
                    query = query.Where(s => s.TenantId == tenantId);
                }

                var dbSessions = await query
                    .OrderByDescending(s => s.LoginTime)
                    .ToListAsync();

                _logger.LogWarning("Falling back to database, retrieved {Count} sessions", dbSessions.Count);
                return dbSessions;
            }
        }

        public async Task<UserSession?> GetUserActiveSessionAsync(string userId)
        {
            return await _context.UserSessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);
        }

        public async Task<List<UserSession>> GetUserSessionHistoryAsync(string userId, int days = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);

            return await _context.UserSessions
                .Where(s => s.UserId == userId && s.LoginTime >= cutoffDate)
                .OrderByDescending(s => s.LoginTime)
                .ToListAsync();
        }

        public async Task ForceLogoutAsync(Guid sessionId)
        {
            try
            {
                // End Redis session (primary)
                await _redisSessionService.EndSessionAsync(sessionId.ToString());

                // Also end database session for historical accuracy
                var session = await _context.UserSessions
                    .FirstOrDefaultAsync(s => s.Oid == sessionId && s.IsActive);

                if (session != null)
                {
                    session.IsActive = false;
                    session.LogoutTime = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Force logout for session {SessionId} user {UserId}", sessionId, session.UserId);
                }
                else
                {
                    _logger.LogInformation("Force logout for Redis session {SessionId}", sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error force logging out session {SessionId}", sessionId);
                throw;
            }
        }

        public async Task CleanupExpiredSessionsAsync(int timeoutMinutes = 30)
        {
            try
            {
                // Cleanup Redis sessions (primary)
                await _redisSessionService.CleanupExpiredSessionsAsync(timeoutMinutes);

                // Cleanup database sessions (secondary, for historical accuracy)
                var cutoffTime = DateTime.UtcNow.AddMinutes(-timeoutMinutes);

                var expiredSessions = await _context.UserSessions
                    .Where(s => s.IsActive && s.LastActivityTime < cutoffTime)
                    .ToListAsync();

                foreach (var session in expiredSessions)
                {
                    session.IsActive = false;
                    session.LogoutTime = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} expired sessions from database", expiredSessions.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired sessions");
                throw;
            }
        }

        public async Task<int> GetActiveUserCountAsync(int? tenantId = null)
        {
            try
            {
                // Get count from Redis (real-time)
                return await _redisSessionService.GetActiveUserCountAsync(tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active user count from Redis, falling back to database");

                // Fallback to database
                var query = _context.UserSessions.Where(s => s.IsActive);

                if (tenantId.HasValue)
                {
                    query = query.Where(s => s.TenantId == tenantId);
                }

                return await query.CountAsync();
            }
        }

        public async Task<Dictionary<string, object>> GetSessionAnalyticsAsync(int? tenantId = null, int days = 7)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var query = _context.UserSessions.Where(s => s.LoginTime >= cutoffDate);

            if (tenantId.HasValue)
            {
                query = query.Where(s => s.TenantId == tenantId);
            }

            var sessions = await query.ToListAsync();

            return new Dictionary<string, object>
            {
                ["TotalSessions"] = sessions.Count,
                ["ActiveSessions"] = sessions.Count(s => s.IsActive),
                ["UniqueUsers"] = sessions.Select(s => s.UserId).Distinct().Count(),
                ["AverageSessionMinutes"] = sessions.Where(s => s.LogoutTime.HasValue)
                    .Average(s => s.SessionDurationMinutes),
                ["TotalActiveMinutes"] = sessions.Sum(s => s.SessionDurationMinutes),
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

        public async Task<UserActivityAnalytics> GetUserActivityAnalyticsAsync(int? tenantId, DateTimeOffset fromDate, DateTimeOffset toDate)
        {
            var query = _context.UserSessions.Where(s => s.LoginTime >= fromDate && s.LoginTime <= toDate.AddDays(1));

            if (tenantId.HasValue)
            {
                query = query.Where(s => s.TenantId == tenantId);
            }

            var sessions = await query.ToListAsync();

            var totalUsers = sessions.Select(s => s.UserId).Distinct().Count();
            var activeSessions = sessions.Where(s => s.IsActive).ToList();
            var activeUsers = activeSessions.Select(s => s.UserId).Distinct().Count();
            var totalSessions = sessions.Count;
            var totalMinutes = sessions.Sum(s => s.SessionDurationMinutes);
            var totalHours = totalMinutes / 60.0;
            var avgSessionHours = totalSessions > 0 ? totalMinutes / (double)totalSessions / 60.0 : 0;

            // Group by date for daily activity
            var dailyActivity = sessions
                .GroupBy(s => s.LoginTime.Date)
                .Select(g => new DailyActivitySummary
                {
                    Date = g.Key,
                    UniqueUsers = g.Select(s => s.UserId).Distinct().Count(),
                    TotalSessions = g.Count(),
                    TotalHours = g.Sum(s => s.SessionDurationMinutes) / 60.0,
                    AverageSessionDuration = g.Average(s => s.SessionDurationMinutes) / 60.0
                })
                .OrderBy(d => d.Date)
                .ToList();

            return new UserActivityAnalytics
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                AverageSessionDurationHours = avgSessionHours,
                TotalSessions = totalSessions,
                TotalHoursLogged = totalHours,
                DailyActivity = dailyActivity
            };
        }

        public async Task<IEnumerable<UserActivitySummary>> GetUserActivitySummaryAsync(int? tenantId, DateTimeOffset fromDate, DateTimeOffset toDate)
        {
            var query = _context.UserSessions.Where(s => s.LoginTime >= fromDate && s.LoginTime <= toDate.AddDays(1));

            if (tenantId.HasValue)
            {
                query = query.Where(s => s.TenantId == tenantId);
            }

            var sessions = await query.ToListAsync();

            var userSummaries = sessions
                .GroupBy(s => new { s.UserId, s.UserName })
                .Select(g => new UserActivitySummary
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.UserName,
                    TotalSessions = g.Count(),
                    TotalHoursLogged = g.Sum(s => s.SessionDurationMinutes) / 60.0,
                    AverageSessionDurationHours = g.Average(s => s.SessionDurationMinutes) / 60.0,
                    LastLoginTime = g.Max(s => s.LoginTime),
                    FirstLoginTime = g.Min(s => s.LoginTime),
                    DaysActive = g.Select(s => s.LoginTime.Date).Distinct().Count(),
                    MostUsedDevice = g.GroupBy(s => s.DeviceInfo)
                        .Where(dg => !string.IsNullOrEmpty(dg.Key))
                        .OrderByDescending(dg => dg.Count())
                        .FirstOrDefault()?.Key,
                    MostUsedLocation = g.GroupBy(s => s.Location)
                        .Where(lg => !string.IsNullOrEmpty(lg.Key))
                        .OrderByDescending(lg => lg.Count())
                        .FirstOrDefault()?.Key
                })
                .OrderByDescending(u => u.TotalHoursLogged).ToList();

            return userSummaries.AsEnumerable();
        }
    }
}
