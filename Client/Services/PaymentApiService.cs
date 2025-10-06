using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.DTOs;

namespace Vanigam.CRM.Client;

public class PaymentApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<Payment>(navigationManager, httpClient, authenticationStateProvider, configuration, nameof(VanigamAccountingDbContext.Payments))
{
    public async Task<Payment?> BulkSavePaymentWithAllocationsAsync(PaymentBulkSaveDTO paymentData)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        if (!authState.User.Identity?.IsAuthenticated == true)
        {
            navigationManager.NavigateTo("/authentication/login");
            return null;
        }

        try
        {
            var json = JsonSerializer.Serialize(paymentData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"odata/VanigamAccountingService/payments/bulk-save", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Payment>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in BulkSavePaymentWithAllocationsAsync: {ex.Message}");
            throw;
        }
    }
}