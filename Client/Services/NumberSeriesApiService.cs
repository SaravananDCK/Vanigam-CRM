using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client;

public class NumberSeriesApiService(
    NavigationManager navigationManager,
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider,
    IConfiguration configuration)
    : BaseApiService<NumberSeries>(navigationManager, httpClient, authenticationStateProvider, configuration, nameof(VanigamAccountingDbContext.NumberSeries))
{
    /// <summary>
    /// Generates the next number for the specified entity type
    /// </summary>
    /// <param name="entityType">The entity type (e.g., "Quote", "Invoice", "PurchaseOrder", "PurchaseInvoice")</param>
    /// <returns>The formatted next number</returns>
    public async Task<string?> GenerateNextNumber(string entityType)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                $"odata/VanigamAccountingService/NumberSeries/GenerateNextNumber",
                new { EntityType = entityType });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GenerateNumberResponse>();
                return result?.Number;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating next number: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Previews the next number without incrementing the series
    /// </summary>
    /// <param name="entityType">The entity type</param>
    /// <returns>The formatted next number preview</returns>
    public async Task<string?> PreviewNextNumber(string entityType)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"odata/VanigamAccountingService/NumberSeries/PreviewNextNumber?entityType={Uri.EscapeDataString(entityType)}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PreviewNumberResponse>();
                return result?.Preview;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error previewing next number: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Resets the number series to its starting number
    /// </summary>
    /// <param name="entityType">The entity type to reset</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> ResetNumberSeries(string entityType)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                $"odata/VanigamAccountingService/NumberSeries/ResetNumberSeries",
                new { EntityType = entityType });

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resetting number series: {ex.Message}");
            return false;
        }
    }

    private class GenerateNumberResponse
    {
        public string Number { get; set; } = string.Empty;
    }

    private class PreviewNumberResponse
    {
        public string Preview { get; set; } = string.Empty;
    }
}
