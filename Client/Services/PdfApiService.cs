using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Vanigam.CRM.Client.Services;

public class PdfApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IJSRuntime jsRuntime)
{
    private async Task<bool> CheckAuthenticationAsync()
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity?.IsAuthenticated == true)
        {
            navigationManager.NavigateTo("/authentication/login");
            return false;
        }
        return true;
    }

    public async Task DownloadQuotePdfAsync(Guid quoteId)
    {
        if (!await CheckAuthenticationAsync()) return;

        try
        {
            var response = await httpClient.GetAsync($"api/pdf/quote/{quoteId}");

            if (response.IsSuccessStatusCode)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"Quote_{quoteId:N}.pdf";

                await jsRuntime.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                navigationManager.NavigateTo("/authentication/login");
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
        if (!await CheckAuthenticationAsync()) return;

        try
        {
            var response = await httpClient.GetAsync($"api/pdf/invoice/{invoiceId}");

            if (response.IsSuccessStatusCode)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"Invoice_{invoiceId:N}.pdf";

                await jsRuntime.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                navigationManager.NavigateTo("/authentication/login");
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

    public async Task PreviewQuotePdfAsync(Guid quoteId)
    {
        if (!await CheckAuthenticationAsync()) return;

        try
        {
            var response = await httpClient.GetAsync($"api/pdf/quote/{quoteId}/preview");

            if (response.IsSuccessStatusCode)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();

                await jsRuntime.InvokeVoidAsync("openPdfInNewTab", pdfBytes);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                navigationManager.NavigateTo("/authentication/login");
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
        if (!await CheckAuthenticationAsync()) return;

        try
        {
            var response = await httpClient.GetAsync($"api/pdf/invoice/{invoiceId}/preview");

            if (response.IsSuccessStatusCode)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();

                await jsRuntime.InvokeVoidAsync("openPdfInNewTab", pdfBytes);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                navigationManager.NavigateTo("/authentication/login");
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
}