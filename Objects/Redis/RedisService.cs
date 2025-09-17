using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Vanigam.CRM.Objects.Redis
{
    public class RedisService
    {
        private readonly ConnectionMultiplexer _redisConnection;
        private readonly IDatabase _database;
        private readonly ConcurrentDictionary<string, IList<Claim>> _claimsCache = new();
        
        public IDatabase Database => _database; // Expose database for other services
        
        public RedisService(IOptions<RedisSettings> redisSettings)
        {
            var connectionString = redisSettings.Value.ConnectionString; // Access the connection string from RedisSettings
            _redisConnection = ConnectionMultiplexer.Connect(connectionString);
            _database = _redisConnection.GetDatabase();
        }

        // Store the user's claims in Redis
        public async Task StoreUserClaimsAsync(string userId, IList<Claim> claims)
        {
            var claimsKey = $"user:{userId}:claims";
            var claimPairs = claims.Select(c => new HashEntry(c.Type, c.Value)).ToArray();
            await _database.HashSetAsync(claimsKey, claimPairs);
            _claimsCache[userId] = claims;
            // Expire user claims after 30 minutes
            //await _database.KeyExpireAsync(claimsKey, TimeSpan.FromMinutes(1));
        }
        // Retrieve the user's claims from Redis
        public async Task<IList<Claim>> GetUserClaimsAsync(string userId)
        {
            if (_claimsCache.TryGetValue(userId, out var cachedClaims))
            {
                return cachedClaims;
            }

            // Retrieve from Redis
            var claimsKey = $"user:{userId}:claims";
            var claimsData = await _database.HashGetAllAsync(claimsKey);
            var claims = claimsData.Select(x => new Claim(x.Name, x.Value)).ToList();

            // Update internal cache
            _claimsCache[userId] = claims;

            return claims;

        }

        // Remove the user's claims from Redis (e.g., during logout or session expiry)
        public async Task RemoveUserClaimsAsync(string userId)
        {
            var claimsKey = $"user:{userId}:claims";
            await _database.KeyDeleteAsync(claimsKey);
        }
        public async Task SetItemAsync(string key, string value)
        {
            await _database.StringSetAsync(key, value);
        }
        public async Task<string> GetItemAsync(string key)
        {
            return await _database.StringGetAsync(key);
        }
        public async Task RemoveItemAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

    }

}

