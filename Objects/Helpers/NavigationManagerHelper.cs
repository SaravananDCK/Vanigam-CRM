using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Vanigam.Objects.Helpers;

namespace Vanigam.CRM.Helpers
{
  
    public static class NavigationManagerHelper
    {
        public static async Task ExportVaccinationsToExcel(this NavigationManager navigationManager, Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/entities/vaccinations/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/entities/vaccinations/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public static async Task ExportVaccinationsToCSV(this NavigationManager navigationManager, Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/entities/vaccinations/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/entities/vaccinations/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public static async Task<dynamic> OpenDialogAsync<T>(this DialogService dialogService,
            string title, Dictionary<string, dynamic> parameters = null, int width = 75, int height = 100)
            where T : ComponentBase
        {
            return await ((VanigamAccountingDialogService)dialogService).OpenCustomDialogAsync<T>(title, parameters,width,height);
        }
        public static async Task<dynamic> OpenDialogWithOutHeaderAsync<T>(this DialogService dialogService,
            string title, Dictionary<string, dynamic> parameters = null, int width = 75, int height = 100)
            where T : ComponentBase
        {
            return await ((VanigamAccountingDialogService)dialogService).OpenCustomDialogWithOutHeaderAsync<T>(title, parameters, width, height);
        }

        public static void CloseDialog(this DialogService dialogService, dynamic result = null)
        {
            ((VanigamAccountingDialogService)dialogService).CloseCustomDialog(result);
        }
    }
}

