using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Vanigam.CRM.Objects.Redis
{
    public class RedisPermissionAuthorizeView<T> : ComponentBase where T : class
    {
        private AuthenticationState currentAuthenticationState;
        private bool? isAuthorized;

        /// <summary>
        /// The content that will be displayed if the user is authorized.
        /// </summary>
        [Parameter] public RenderFragment<AuthenticationState> ChildContent { get; set; }

        /// <summary>
        /// The content that will be displayed if the user is not authorized.
        /// </summary>
        [Parameter] public RenderFragment<AuthenticationState> NotAuthorized { get; set; }

        /// <summary>
        /// The content that will be displayed if the user is authorized.
        /// If you specify a value for this parameter, do not also specify a value for <see cref="ChildContent"/>.
        /// </summary>
        [Parameter] public RenderFragment<AuthenticationState> Authorized { get; set; }

        /// <summary>
        /// The content that will be displayed while asynchronous authorization is in progress.
        /// </summary>
        [Parameter] public RenderFragment Authorizing { get; set; }

        /// <summary>
        /// The resource to which access is being controlled.
        /// </summary>
        [Parameter] public PermissionType PermissionType { get; set; }

        [CascadingParameter] private Task<AuthenticationState> AuthenticationState { get; set; }

        [Inject] private HttpClient HttpClient { get; set; }

        static Dictionary<string, bool> AuthDictionary = new Dictionary<string, bool>();
        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            // We're using the same sequence number for each of the content items here
            // so that we can update existing instances if they are the same shape
            if (isAuthorized == null)
            {
                builder.AddContent(0, Authorizing);
            }
            else if (isAuthorized == true)
            {
                var authorized = Authorized ?? ChildContent;
                builder.AddContent(0, authorized?.Invoke(currentAuthenticationState!));
            }
            else
            {
                builder.AddContent(0, NotAuthorized?.Invoke(currentAuthenticationState!));
            }
        }

        /// <inheritdoc />
        protected override async Task OnParametersSetAsync()
        {
            // We allow 'ChildContent' for convenience in basic cases, and 'Authorized' for symmetry
            // with 'NotAuthorized' in other cases. Besides naming, they are equivalent. To avoid
            // confusion, explicitly prevent the case where both are supplied.
            if (ChildContent != null && Authorized != null)
            {
                throw new InvalidOperationException($"Do not specify both '{nameof(Authorized)}' and '{nameof(ChildContent)}'.");
            }

            if (AuthenticationState == null)
            {
                throw new InvalidOperationException($"Authorization requires a cascading parameter of type Task<{nameof(AuthenticationState)}>. Consider using {typeof(CascadingAuthenticationState).Name} to supply this.");
            }

            // Clear the previous result of authorization
            // This will cause the Authorizing state to be displayed until the authorization has been completed
            isAuthorized = null;

            currentAuthenticationState = await AuthenticationState;
            isAuthorized = await IsAuthorizedAsync(currentAuthenticationState.User);
        }

        /// <summary>
        /// Gets the data required to apply authorization rules.
        /// </summary>


        private async Task<bool> IsAuthorizedAsync(ClaimsPrincipal user)
        {
            var userId = user.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }
            string key = $"Redis/HasPermission?userId={userId}&tableName={GetDbSetPropertyName()}&permissionType={PermissionType}";
            if (AuthDictionary.ContainsKey(key))
            {
                return AuthDictionary[key];
            }
            var response = await HttpClient.GetFromJsonAsync<bool>(key);
            if (!AuthDictionary.ContainsKey(key))  AuthDictionary.Add(key, response);
            return response;
        }
        private string GetDbSetPropertyName()
        {
            var dbSetType = typeof(DbSet<T>);
            var properties = typeof(VanigamAccountingDbContext).GetProperties();

            foreach (var property in properties)
            {
                if (property.PropertyType == dbSetType)
                {
                    return property.Name;
                }
            }

            return null; // or throw an exception if not found
        }
    }
    [Flags]
    public enum PermissionType
    {
        Read = 1,
        Create = 2,
        Update = 4,
        Delete = 8
    }

}

