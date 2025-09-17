using System.Security.Claims;

namespace Vanigam.CRM.Server.ApiAuth;

public interface IApiKeyValidation
{
    bool IsValidApiKey(string userApiAccountId,string userApiToken);
}
