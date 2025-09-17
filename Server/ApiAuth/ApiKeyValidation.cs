using System.Security.Claims;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Vanigam.CRM.Server.ApiAuth;

public class ApiKeyValidation : IApiKeyValidation
{
    private IConfiguration Configuration { get; }
    public VanigamAccountingDbContext DbContext { get; }

    public ApiKeyValidation(IConfiguration configuration, VanigamAccountingDbContext dbContext)
    {
        Configuration = configuration;
        DbContext = dbContext;
    }
    public bool IsValidApiKey(string userApiAccountId, string userApiToken)
    {
        if (string.IsNullOrWhiteSpace(userApiAccountId) || string.IsNullOrWhiteSpace(userApiToken))
            return false;
        Guid userApiAccountGuId=Guid.Empty;
        Guid.TryParse(userApiAccountId, out userApiAccountGuId);
        if (userApiAccountGuId == Guid.Empty)
        {
            return false;
        }
        var loginUser = DbContext.Users.FirstOrDefault(u => u.ApiAccountId == Guid.Parse(userApiAccountId));
        if (loginUser == null) return false;
        return (loginUser.ApiAccountId.ToString() == userApiAccountId &&
                userApiToken == $"Bearer {loginUser.ApiAuthToken}");

    }
}

