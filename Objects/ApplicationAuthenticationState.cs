using System.Globalization;
using Vanigam.CRM.Objects;

namespace Vanigam.CRM.Objects
{

    public partial class ApplicationAuthenticationState
    {
        public string Token { get; set; }
        public bool TokenExpired { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }
        public IEnumerable<ApplicationClaim> Claims { get; set; }
        public string GetBearerToken()
        {
            var tokenClaimExpiry = Claims?.FirstOrDefault(c => c.Type == "Bearer_Token_Expiry");
            if (!string.IsNullOrEmpty(tokenClaimExpiry?.Value))
            {
                if (DateTime.Parse(tokenClaimExpiry.Value.ToString(), new CultureInfo("en-US")) > DateTime.UtcNow)
                {
                    var tokenClaim = Claims?.FirstOrDefault(c => c.Type == "Bearer_Token");
                    if (tokenClaim != null)
                    {
                        return tokenClaim.Value;
                    }
                }
            }
            return null;
        }
    }
}
