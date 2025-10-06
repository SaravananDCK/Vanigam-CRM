using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Text.Json;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.DTOs;

namespace Vanigam.CRM.Client;

public class QuoteApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<Quote>(navigationManager, httpClient, authenticationStateProvider, configuration, nameof(VanigamAccountingDbContext.Quotes))
{
    public async Task<Quote?> BulkSaveQuoteWithItemsAsync(QuoteBulkSaveDTO quoteData)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity?.IsAuthenticated == true)
        {
            navigationManager.NavigateTo("/authentication/login");
            return null;
        }

        try
        {
            var json = JsonSerializer.Serialize(quoteData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"odata/VanigamAccountingService/quotes/bulk-save", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Quote>(responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to save quote: {errorMessage}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Network error: {ex.Message}");
        }
    }

    public async Task<List<QuoteItemDTO>> GetQuoteItemsForEditingAsync(Guid quoteId)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity?.IsAuthenticated == true)
        {
            navigationManager.NavigateTo("/authentication/login");
            return new List<QuoteItemDTO>();
        }

        try
        {
            var response = await httpClient.GetAsync($"api/quote/{quoteId}/items-for-editing");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<QuoteItemDTO>>(responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new List<QuoteItemDTO>();
            }
            else
            {
                throw new InvalidOperationException($"Failed to load quote items: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Network error: {ex.Message}");
        }
    }

    public async Task<StatusSummaryResponse<QuoteStatus>> GetStatusSummaryAsync(StatusSummaryRequest request)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                $"odata/VanigamAccountingService/Quotes/status-summary",
                request);

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<StatusSummaryResponse<QuoteStatus>>(json, GetJsonSerializerOptions())
                   ?? new StatusSummaryResponse<QuoteStatus>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting quote status summary: {ex.Message}");
            return new StatusSummaryResponse<QuoteStatus>
            {
                TotalCount = 0,
                StatusCounts = new Dictionary<QuoteStatus, int>()
            };
        }
    }

    public async Task<Invoice?> ConvertQuoteToInvoiceAsync(Guid quoteId, string? invoiceNumber = null)
    {
        try
        {
            var request = new ConvertQuoteToInvoiceRequest
            {
                InvoiceNumber = invoiceNumber
            };

            var response = await httpClient.PostAsJsonAsync(
                $"api/quote/{quoteId}/convert-to-invoice",
                request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Invoice>(json, GetJsonSerializerOptions());
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to convert quote to invoice: {errorMessage}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Network error: {ex.Message}");
        }
    }
}

public class ConvertQuoteToInvoiceRequest
{
    public string? InvoiceNumber { get; set; }
}