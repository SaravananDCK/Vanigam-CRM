using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Enums;

namespace Vanigam.CRM.Objects.Services
{
    public partial interface ICurrentUserService
    {
        string UserId { get; }
        string BehalfOfProviderId { get; }
        string UserName { get; }
        string Email { get; }
        string FullName { get; }
        int? TenantId { get; }
        LoginUserType? UserType { get; }
        List<KeyValuePair<string, string>> Claims { get; set; }
    }

    public class TestCurrentUserService : ICurrentUserService
    {
        public TestCurrentUserService()
        { }
        public TestCurrentUserService(string userId)
        {
            UserId = userId;
        }
        public string UserId { get; }
        public string BehalfOfProviderId { get; }
        public string UserName { get; }
        public string Email { get; }
        public string FullName { get; }
        public int? TenantId { get; }
        public LoginUserType? UserType { get; }
        public List<KeyValuePair<string, string>> Claims { get; set; }
    }
    
    public partial class CurrentUserService : ICurrentUserService
    {
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor.HttpContext != null)
            {
                UserId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                Claims = httpContextAccessor.HttpContext?.User?.Claims.AsEnumerable()
                    .Select(item => new KeyValuePair<string, string>(item.Type, item.Value)).ToList();
                var tenantStr = httpContextAccessor.HttpContext?.User?.Claims
                    .FirstOrDefault(c => c.Type == nameof(ApplicationUser.TenantId))?.Value;
                if (!string.IsNullOrEmpty(tenantStr))
                {
                    TenantId = int.Parse(tenantStr);
                }

                var userStr = httpContextAccessor.HttpContext?.User?.Claims
                    .FirstOrDefault(c => c.Type == nameof(ApplicationUser.UserName))?.Value;
                if (!string.IsNullOrEmpty(userStr))
                {
                    UserName = userStr;
                }

                var emailStr = httpContextAccessor.HttpContext?.User?.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(emailStr))
                {
                    Email = emailStr;
                }

                var fullNameStr = httpContextAccessor.HttpContext?.User?.Claims
                    .FirstOrDefault(c => c.Type == nameof(ApplicationUser.FullName))?.Value;
                if (!string.IsNullOrEmpty(fullNameStr))
                {
                    FullName = fullNameStr;
                }


                var userTypeStr = httpContextAccessor.HttpContext?.User?.Claims
                    .FirstOrDefault(c => c.Type == nameof(LoginUserType))?.Value;
                if (!string.IsNullOrEmpty(userTypeStr))
                {
                    UserType = (LoginUserType?)Enum.Parse(typeof(LoginUserType), userTypeStr);
                }

            }
            else
            {
                //UserId = User.SystemUserId.ToString();
                //UserName = User.SystemUserName;
                //FullName = User.SystemUserName;
                //UserType = LoginUserType.SuperUser;
            }
        }

        public string UserId { get; }
        public string BehalfOfProviderId { get; }
        public string UserName { get; }
        public string Email { get; }
        public string FullName { get; }
        public int? TenantId { get; }
        public LoginUserType? UserType { get; }
        public List<KeyValuePair<string, string>> Claims { get; set; }
    }

}

