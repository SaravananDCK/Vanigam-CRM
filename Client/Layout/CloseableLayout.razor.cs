using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace Vanigam.CRM.Client.Layout
{
    public partial class CloseableLayout
    {
        
        string GetLogoPath()
        {
            return @$"images\{Security.User.TenantId}\Logo.svg";
        }
        private string ReturnUrl { get; set; }
        public void UpdateUrl(string url)
        {
            ReturnUrl=url;
        }
        public async Task OnCloseClick()
        {
            NavigationManager.NavigateTo(@$"\{ReturnUrl}");
        }
        protected static Version AppVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? throw new NullReferenceException();
        public static readonly int DateTimeNowYear = DateTime.Now.Year;
        protected override void OnInitialized()
        {
            base.OnInitialized();
            Assembly curAssembly = typeof(Program).Assembly;
            BuildInfo = $"{curAssembly.GetCustomAttributes(false).OfType<AssemblyTitleAttribute>().FirstOrDefault().Title}";
            ComponentsInfo = $"{typeof(RadzenComponent).Assembly.GetName().Version.ToString()}";
        }
    }
}

