using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client;

public class PurchaseOrderApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<PurchaseOrder>(navigationManager, httpClient, authenticationStateProvider, configuration, nameof(VanigamAccountingDbContext.PurchaseOrders))
{
    public async Task<PurchaseOrder?> BulkSavePurchaseOrderWithItemsAsync(PurchaseOrderBulkSaveDTO purchaseData)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity?.IsAuthenticated == true)
        {
            navigationManager.NavigateTo("/authentication/login");
            return null;
        }

        try
        {
            var json = JsonSerializer.Serialize(purchaseData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"odata/VanigamAccountingService/{nameof(VanigamAccountingDbContext.PurchaseOrders)}/bulk-save", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PurchaseOrder>(responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to save Puchase Order: {errorMessage}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Network error: {ex.Message}");
        }
    }

    public async Task<List<PurchaseOrderItemDTO>> GetPurchaseOrderItemsForEditingAsync(Guid purchaseOrderId)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity?.IsAuthenticated == true)
        {
            navigationManager.NavigateTo("/authentication/login");
            return new List<PurchaseOrderItemDTO>();
        }

        try
        {
            var response = await httpClient.GetAsync($"api/purchaseOrder/{purchaseOrderId}/items-for-editing");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<PurchaseOrderItemDTO>>(responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new List<PurchaseOrderItemDTO>();
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
}
