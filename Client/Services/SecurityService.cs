using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Enums;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Helpers;

namespace Vanigam.CRM.Client
{
    public partial class SecurityService(NavigationManager navigationManager, IHttpClientFactory factory)
    {

        private readonly HttpClient httpClient = factory.CreateClient("Vanigam.CRM.AI.Server");

        private readonly Uri baseUri = new($"{navigationManager.BaseUri}odata/Identity/");

        public ApplicationUser User { get; private set; } = new ApplicationUser { Name = "Anonymous" };

        public ClaimsPrincipal Principal { get; private set; }

        public LayoutMode? LayoutMode { get; private set; }
        private bool IsDevelopment()
        {
            //#if DEBUG
            //            return true;
            //#else
            return false;
            //#endif
        }

        public bool IsInRole(params string[] roles)
        {
            //#if DEBUG
            //            if (User?.Name == "admin")
            //            {
            //                return true;
            //            }
            //#endif

            if (roles.Contains("Everybody"))
            {
                return true;
            }

            if (!IsAuthenticated())
            {
                return false;
            }

            if (roles.Contains("Authenticated"))
            {
                return true;
            }

            return roles.Any(role => Principal.IsInRole(role));
        }

        public bool IsAuthenticated()
        {
            return Principal?.Identity.IsAuthenticated == true;
        }
        public LoginUserType? UserType { get; private set; }
        public async Task<bool> InitializeAsync(AuthenticationState result)
        {
            Principal = result.User;

            //#if DEBUG
            //            if (Principal.Identity.Name == "admin")
            //            {
            //                User = new ApplicationUser { Name = ApplicationUser.Admin };

            //                return true;
            //            }
            //#endif
            var userId = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null && User?.Id.ToString() != userId)
            {
                User = await GetUserById(userId);
                UserType = User?.UserType;
                LayoutMode = User?.LayoutMode;
                if (User != null && Tenant == null)
                {
                    if (IsDevelopment() && User.Name == ApplicationUser.Admin || User.Name == ApplicationUser.TenantsAdmin)
                    {
                        var tenants = await GetTenants();
                        if (tenants.Any())
                        {
                            Tenant = tenants.FirstOrDefault();
                        }
                    }
                    else if (User.TenantId != null)
                    {
                        User.ApplicationTenant = await GetTenantById(User.TenantId.Value);
                        Tenant = User.ApplicationTenant;
                    }
                }
            }

            return IsAuthenticated();
        }


        public ApplicationTenant Tenant { get; set; }

        public async Task<ApplicationAuthenticationState> GetAuthenticationStateAsync()
        {
            var uri = new Uri($"{navigationManager.BaseUri}Account/CurrentUser");
            var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, uri));
            var responseState = await VanigamAccountingHttpResponseMessageExtensions.ReadAsync<ApplicationAuthenticationState>(response);

            if (responseState.TokenExpired)
            {
                Logout();
            }
            return responseState;
        }
        public async Task<VanigamAccountingOptions> GetVanigamAccountingOptions()
        {
            var uri = new Uri($"{navigationManager.BaseUri}Account/GetVanigamAccountingOptions");
            var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri));
            var vanigamAccountingOptions = await VanigamAccountingHttpResponseMessageExtensions.ReadAsync<VanigamAccountingOptions>(response);
            return vanigamAccountingOptions;
        }

        public void Logout()
        {
            navigationManager.NavigateTo("Account/Logout", true);
        }

        public void Login()
        {
            navigationManager.NavigateTo("Login", true);
        }

        public async Task<IEnumerable<ApplicationRole>> GetRoles()
        {
            var uri = new Uri(baseUri, $"ApplicationRoles");

            //uri = uri.GetODataUri(filter: $"TenantId eq {Tenant.Id}");

            var response = await httpClient.GetAsync(uri);

            var result = await HttpResponseMessageExtensions.ReadAsync<ODataServiceResult<ApplicationRole>>(response);

            return result.Value;
        }

        public async Task<ApplicationRole> CreateRole(ApplicationRole role)
        {

            if (Tenant != null)
            {
                role.TenantId = Tenant.Id;
            }
            var uri = new Uri(baseUri, $"ApplicationRoles");

            var content = new StringContent(ODataJsonSerializer.Serialize(role), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(uri, content);

            return await HttpResponseMessageExtensions.ReadAsync<ApplicationRole>(response);
        }
        public async Task SwitchTenantAndReAuthenticate(string userId, int tenantId)
        {
            var uri = new Uri($"{navigationManager.BaseUri}Account/SwitchTenantAndReAuthenticate");

            var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "userId", userId },
                { "tenantId", tenantId.ToString() }
            });

            var response = await httpClient.PostAsync(uri, content);

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();

                throw new ApplicationException(message);
            }

        }

        public async Task<HttpResponseMessage> DeleteRole(string id)
        {
            var uri = new Uri(baseUri, $"ApplicationRoles('{id}')");

            return await httpClient.DeleteAsync(uri);
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsers()
        {
            var uri = new Uri(baseUri, $"ApplicationUsers");


            uri = uri.GetODataUri(filter: $"TenantId eq {Tenant.Id}");

            var response = await httpClient.GetAsync(uri);

            var result = await HttpResponseMessageExtensions.ReadAsync<ODataServiceResult<ApplicationUser>>(response);

            return result.Value;
        }

        public async Task<ApplicationUser> CreateUser(ApplicationUser user)
        {

            if (Tenant != null)
            {
                user.TenantId = Tenant.Id;
            }
            var uri = new Uri(baseUri, $"ApplicationUsers");

            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(uri, content);

            return await HttpResponseMessageExtensions.ReadAsync<ApplicationUser>(response);
        }

        public async Task<HttpResponseMessage> DeleteUser(string id)
        {
            var uri = new Uri(baseUri, $"ApplicationUsers('{id}')");

            return await httpClient.DeleteAsync(uri);
        }

        public async Task<ApplicationUser> GetUserById(string id)
        {
            //var uri = new Uri(baseUri, $"ApplicationUsers('{id}')?$expand=Roles");
            var uri = new Uri(baseUri, $"{navigationManager.BaseUri}Account/GetUserById?id={id}");

            var response = await httpClient.GetAsync(uri);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            return await HttpResponseMessageExtensions.ReadAsync<ApplicationUser>(response);
        }
        public async Task<bool?> DeactivateUser(string id)
        {
            var uri = new Uri($"{navigationManager.BaseUri}Account/DeactivateUser?userId={id}");
            uri = uri.GetODataUri(null, null, null, null, null);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            var response = await httpClient.SendAsync(httpRequestMessage);
            return response.IsSuccessStatusCode;
        }
        public async Task<bool?> ActivateUser(string id)
        {
            var uri = new Uri($"{navigationManager.BaseUri}Account/ActivateUser?userId={id}");
            uri = uri.GetODataUri(null, null, null, null, null);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            var response = await httpClient.SendAsync(httpRequestMessage);
            return response.IsSuccessStatusCode;
        }
        //public async Task<User?> GetLoginUser()
        //{
        //    //var uri = new Uri(baseUri, $"ApplicationUsers('{id}')?$expand=Roles");
        //    var uri = new Uri(baseUri, $"{navigationManager.BaseUri}Account/GetLoginUser");

        //    var response = await httpClient.GetAsync(uri);

        //    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        //    {
        //        return null;
        //    }

        //    return await HttpResponseMessageExtensions.ReadAsync<User?>(response);
        //}


        public async Task<ApplicationUser> UpdateUser(string id, ApplicationUser user)
        {
            var uri = new Uri(baseUri, $"ApplicationUsers('{id}')");
            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Patch, uri)
            {
                Content = content
            };
            var stringContent = await content.ReadAsStringAsync();
            var response = await httpClient.SendAsync(httpRequestMessage);

            return await HttpResponseMessageExtensions.ReadAsync<ApplicationUser>(response);
        }
        public async Task UpdateUserTenant(string userId, int tenantId)
        {
            var uri = new Uri($"{navigationManager.BaseUri}Account/UpdateUserTenant");

            var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                    { "userId", userId },
                    { "tenantId", tenantId.ToString() }
                });

            var response = await httpClient.PostAsync(uri, content);

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();

                throw new ApplicationException(message);
            }

        }

        // Tenants
        public async System.Threading.Tasks.Task<IEnumerable<ApplicationTenant>> GetTenants()
        {
            var uri = new Uri(baseUri, $"ApplicationTenants");
            uri = uri.GetODataUri();

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

            var response = await httpClient.SendAsync(httpRequestMessage);

            var result = await HttpResponseMessageExtensions.ReadAsync<ODataServiceResult<ApplicationTenant>>(response);

            return result.Value;
        }

        public async System.Threading.Tasks.Task<ApplicationTenant> CreateTenant(ApplicationTenant tenant = default(ApplicationTenant))
        {
            var uri = new Uri(baseUri, $"ApplicationTenants");

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);

            httpRequestMessage.Content = new StringContent(ODataJsonSerializer.Serialize(tenant), Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(httpRequestMessage);

            return await HttpResponseMessageExtensions.ReadAsync<ApplicationTenant>(response);
        }

        public async System.Threading.Tasks.Task<HttpResponseMessage> DeleteTenant(int id = default(int))
        {
            var uri = new Uri(baseUri, $"ApplicationTenants({id})");

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);

            return await httpClient.SendAsync(httpRequestMessage);
        }

        public async System.Threading.Tasks.Task<IEnumerable<ApplicationUser>> GetTenantUsers(int tenantId = default(int))
        {
            var uri = new Uri(baseUri, $"ApplicationTenants/GetTenantUsers(tenantId={tenantId})");

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

            var response = await httpClient.SendAsync(httpRequestMessage);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            var result = await HttpResponseMessageExtensions.ReadAsync<ODataServiceResult<ApplicationUser>>(response);
            return result.Value;
        }
        public async System.Threading.Tasks.Task<ApplicationTenant> GetTenantById(int id = default(int))
        {
            var uri = new Uri(baseUri, $"ApplicationTenants({id})");

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

            var response = await httpClient.SendAsync(httpRequestMessage);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            return await HttpResponseMessageExtensions.ReadAsync<ApplicationTenant>(response);
        }

        public async System.Threading.Tasks.Task<HttpResponseMessage> UpdateTenant(int id = default(int), ApplicationTenant tenant = default(ApplicationTenant))
        {
            var uri = new Uri(baseUri, $"ApplicationTenants({id})");

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Patch, uri);

            httpRequestMessage.Content = new StringContent(ODataJsonSerializer.Serialize(tenant), Encoding.UTF8, "application/json");

            return await httpClient.SendAsync(httpRequestMessage);
        }
        public async Task ChangePassword(string oldPassword, string newPassword)
        {
            var uri = new Uri($"{navigationManager.BaseUri}Account/ChangePassword");

            var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "oldPassword", oldPassword },
                { "newPassword", newPassword }
            });

            var response = await httpClient.PostAsync(uri, content);

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();

                throw new ApplicationException(message);
            }
        }

        public async Task Register(string userName, string password)
        {
            var uri = new Uri($"{navigationManager.BaseUri}Account/Register");

            var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "userName", userName },
                { "password", password }
            });

            var response = await httpClient.PostAsync(uri, content);

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();

                throw new ApplicationException(message);
            }
        }

        public async Task ResetPassword(string userName)
        {
            var uri = new Uri($"{navigationManager.BaseUri}Account/ResetPassword");

            var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "userName", userName }
            });

            var response = await httpClient.PostAsync(uri, content);

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();

                throw new ApplicationException(message);
            }
        }
    }
}
