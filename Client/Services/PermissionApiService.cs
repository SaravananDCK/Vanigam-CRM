using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Radzen;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.DTOs;

namespace Vanigam.CRM.Client.Services
{
    public class PermissionApiService(NavigationManager navigationManager, IHttpClientFactory factory)
    {
        private readonly HttpClient HttpClient = factory.CreateClient("Vanigam.CRM.AI.Server");

        private readonly Uri baseUri = new($"{navigationManager.BaseUri}Permission/");

        public async Task<IEnumerable<AuthorizationPermission>> GetClaims(string roleId)
        {
            var uri = new Uri($"{navigationManager.BaseUri}Permission/GetClaims(roleId='{roleId}')");
            var response = await HttpClient.GetAsync(uri);
            var result = await HttpResponseMessageExtensions.ReadAsync<IEnumerable<AuthorizationPermission>>(response);
            return result;
        }
       
        public async Task<HttpResponseMessage> UpdatePermissions(AuthorizationPermission permission)
        {
            var uri = new Uri($"{navigationManager.BaseUri}Permission/UpdatePermissions");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            var serializedText = JsonSerializer.Serialize(permission);
            httpRequestMessage.Content = new StringContent(serializedText, Encoding.UTF8, "application/json");
            var response = await HttpClient.SendAsync(httpRequestMessage);
            return response;
        }
    }
}

