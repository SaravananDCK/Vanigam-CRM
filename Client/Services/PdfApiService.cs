using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using Radzen;
using Vanigam.CRM.Helpers;

namespace Vanigam.CRM.Client.Services;

public class PdfApiService
{
    protected readonly HttpClient HttpClient;
    protected readonly ApplicationAuthenticationStateProvider AuthenticationStateProvider;
    protected readonly NavigationManager NavigationManager;
    protected readonly IJSRuntime JSRuntime;
    protected readonly DialogService DialogService;

    public PdfApiService(NavigationManager navigationManager,
        HttpClient httpClient,
        AuthenticationStateProvider authenticationStateProvider,
        IJSRuntime jsRuntime,
        DialogService dialogService)
    {
        HttpClient = httpClient;
        AuthenticationStateProvider = authenticationStateProvider as ApplicationAuthenticationStateProvider;
        NavigationManager = navigationManager;
        JSRuntime = jsRuntime;
        DialogService = dialogService;
        BearerToken = (authenticationStateProvider as ApplicationAuthenticationStateProvider)?.GetBearerToken();
        this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
    }

    public string BearerToken { get; private set; }

    public async Task DownloadQuotePdfAsync(Guid quoteId)
    {

        try
        {
            var response = await HttpClient.GetAsync($"api/pdf/quote/{quoteId}");

            if (response.IsSuccessStatusCode)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"Quote_{quoteId:N}.pdf";

                await JSRuntime.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            else
            {
                Console.WriteLine($"Error downloading quote PDF: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading quote PDF: {ex.Message}");
        }
    }

    public async Task DownloadInvoicePdfAsync(Guid invoiceId)
    {

        try
        {
            var response = await HttpClient.GetAsync($"api/pdf/invoice/{invoiceId}");

            if (response.IsSuccessStatusCode)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"Invoice_{invoiceId:N}.pdf";

                await JSRuntime.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            else
            {
                Console.WriteLine($"Error downloading invoice PDF: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading invoice PDF: {ex.Message}");
        }
    }
    public async Task DownloadPurchaseOrderPdfAsync(Guid purchaseorderId)
    {

        try
        {
            var response = await HttpClient.GetAsync($"api/pdf/purchaseOrder/{purchaseorderId}");

            if (response.IsSuccessStatusCode)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"Purchase Order_{purchaseorderId:N}.pdf";

                await JSRuntime.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            else
            {
                Console.WriteLine($"Error downloading purchase order PDF: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading purchase order PDF: {ex.Message}");
        }
    }

    public async Task PreviewQuotePdfAsync(Guid quoteId)
    {

        try
        {
            var response = await HttpClient.GetAsync($"api/pdf/quote/{quoteId}/preview");

            if (response.IsSuccessStatusCode)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();

                await DialogService.OpenDialogWithOutHeaderAsync<Client.Components.Dialogs.PdfPreviewDialog>("Quote PDF Preview",
                    new Dictionary<string, object>
                    {
                        { "PdfBytes", pdfBytes },
                        { "DialogService", DialogService},
                    },
                    90, 100);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            else
            {
                Console.WriteLine($"Error previewing quote PDF: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error previewing quote PDF: {ex.Message}");
        }
    }

    public async Task PreviewInvoicePdfAsync(Guid invoiceId)
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/pdf/invoice/{invoiceId}/preview");

            if (response.IsSuccessStatusCode)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();

                await DialogService.OpenDialogAsync<Client.Components.Dialogs.PdfPreviewDialog>("Invoice PDF Preview",
                    new Dictionary<string, object>
                    {
                        { "PdfBytes", pdfBytes },
                        { "DialogService", DialogService},

                    },
                    90, 100);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            else
            {
                Console.WriteLine($"Error previewing invoice PDF: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error previewing invoice PDF: {ex.Message}");
        }
    }
    public async Task PreviewPurchaseOrderPdfAsync(Guid purchaseorderId)
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/pdf/purchaseOrder/{purchaseorderId}/preview");

            if (response.IsSuccessStatusCode)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();

                await DialogService.OpenDialogWithOutHeaderAsync<Client.Components.Dialogs.PdfPreviewDialog>("Purchase Order PDF Preview",
                    new Dictionary<string, object>
                    {
                        { "PdfBytes", pdfBytes },
                        { "DialogService", DialogService},
                    },
                    90, 100);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                NavigationManager.NavigateTo("/authentication/login");
            }
            else
            {
                Console.WriteLine($"Error previewing Purchase Order PDF: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error previewing Purchase Order PDF: {ex.Message}");
        }
    }
}