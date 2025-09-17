using Microsoft.AspNetCore.Identity;
using StackExchange.Redis;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Session
{
    public class SessionManager(UserManager<ApplicationUser> userManager, ILogger<SessionManager> logger)
    {
        public ILogger<SessionManager> Logger { get; } = logger;

        public async Task<bool> LogInLock(ApplicationUser user)
        {
            try
            {
                user.SessionId = Guid.NewGuid().ToString();
                await userManager.UpdateAsync(user);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e,e.Message);
            }
            return false;
        }

        public async Task LogOutLock(ApplicationUser user)
        {
            user.SessionId = null;
            await userManager.UpdateAsync(user);
        }
    }
}

