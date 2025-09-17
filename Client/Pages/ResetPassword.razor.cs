using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vanigam.CRM.Objects.Helpers;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Client.Pages.DetailView;
using Vanigam.CRM.Objects.Models;
using Vanigam.CRM.Objects;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Pages
{
    public partial class ResetPassword
    {
        [Parameter] public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        [Inject] protected SecurityService Security { get; set; }
        [Parameter] public string Token { get; set; }

        private ResetPasswordModel ResetPasswordModel = new ResetPasswordModel();
        protected bool IsBusy { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            User = await Security.GetUserById(UserId);
        }

        private async Task SubmitForm()
        {
            IsBusy = true;
            ResetPasswordModel.UserId = UserId;
            ResetPasswordModel.Token = Token;

            //var response = await HttpClient.PostAsJsonAsync($"{NavigationManager.BaseUri}account/resetpassword", resetPasswordModel);
            var result = await HttpClient.PostAsJsonAsync($"account/resetpassword", ResetPasswordModel);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string successMessage = await result.Content.ReadAsStringAsync();
                NavigationManager.NavigateTo($"/Login?info={successMessage}.");
            }
            else
            {
                string errorMessage = await result.Content.ReadAsStringAsync();
                NavigationManager.NavigateTo($"/Login?error={errorMessage}.");
            }
            IsBusy = false;
        }
    }
}
