using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using Vanigam.CRM.Objects.DTOs;

namespace Vanigam.CRM.Client.Services;

public class GSTReportsApiService(
    HttpClient httpClient,
    AuthenticationStateProvider authenticationStateProvider)
{
    private readonly string _baseUrl = "api/GSTReports";

    /// <summary>
    /// Get GSTR-1 report for the specified period
    /// </summary>
    public async Task<GSTR1Report?> GetGSTR1ReportAsync(int month, int year)
    {
        try
        {
            var response = await httpClient.GetAsync($"{_baseUrl}/gstr1?month={month}&year={year}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GSTR1Report>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching GSTR-1 report: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Get GSTR-3B report for the specified period
    /// </summary>
    public async Task<GSTR3BReport?> GetGSTR3BReportAsync(int month, int year)
    {
        try
        {
            var response = await httpClient.GetAsync($"{_baseUrl}/gstr3b?month={month}&year={year}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GSTR3BReport>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching GSTR-3B report: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Get HSN Summary for the specified period
    /// </summary>
    public async Task<List<HSNSummary>?> GetHSNSummaryAsync(int month, int year)
    {
        try
        {
            var response = await httpClient.GetAsync($"{_baseUrl}/hsn-summary?month={month}&year={year}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<HSNSummary>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching HSN Summary: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Download GSTR-1 report as JSON file
    /// </summary>
    public string GetGSTR1JsonDownloadUrl(int month, int year)
    {
        return $"{_baseUrl}/gstr1/export/json?month={month}&year={year}";
    }

    /// <summary>
    /// Download GSTR-3B report as JSON file
    /// </summary>
    public string GetGSTR3BJsonDownloadUrl(int month, int year)
    {
        return $"{_baseUrl}/gstr3b/export/json?month={month}&year={year}";
    }
}
