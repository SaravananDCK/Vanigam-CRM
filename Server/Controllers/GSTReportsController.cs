using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class GSTReportsController(
    GSTR1ReportService gstr1Service,
    GSTR3BReportService gstr3BService,
    ILogger<GSTReportsController> logger) : ControllerBase
{
    /// <summary>
    /// Generate GSTR-1 report for the specified period
    /// </summary>
    /// <param name="month">Month (1-12)</param>
    /// <param name="year">Year (e.g., 2025)</param>
    /// <returns>GSTR-1 report data</returns>
    [HttpGet("gstr1")]
    public async Task<ActionResult<GSTR1Report>> GetGSTR1Report([FromQuery] int month, [FromQuery] int year)
    {
        try
        {
            if (month < 1 || month > 12)
            {
                return BadRequest("Month must be between 1 and 12");
            }

            if (year < 2000 || year > DateTime.Now.Year + 1)
            {
                return BadRequest("Invalid year");
            }

            var report = await gstr1Service.GenerateGSTR1ReportAsync(month, year);
            return Ok(report);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating GSTR-1 report for {Month}/{Year}", month, year);
            return StatusCode(500, "An error occurred while generating the report");
        }
    }

    /// <summary>
    /// Generate GSTR-3B report for the specified period
    /// </summary>
    /// <param name="month">Month (1-12)</param>
    /// <param name="year">Year (e.g., 2025)</param>
    /// <returns>GSTR-3B report data</returns>
    [HttpGet("gstr3b")]
    public async Task<ActionResult<GSTR3BReport>> GetGSTR3BReport([FromQuery] int month, [FromQuery] int year)
    {
        try
        {
            if (month < 1 || month > 12)
            {
                return BadRequest("Month must be between 1 and 12");
            }

            if (year < 2000 || year > DateTime.Now.Year + 1)
            {
                return BadRequest("Invalid year");
            }

            var report = await gstr3BService.GenerateGSTR3BReportAsync(month, year);
            return Ok(report);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating GSTR-3B report for {Month}/{Year}", month, year);
            return StatusCode(500, "An error occurred while generating the report");
        }
    }

    /// <summary>
    /// Generate HSN Summary for the specified period
    /// </summary>
    /// <param name="month">Month (1-12)</param>
    /// <param name="year">Year (e.g., 2025)</param>
    /// <returns>HSN Summary data</returns>
    [HttpGet("hsn-summary")]
    public async Task<ActionResult<List<HSNSummary>>> GetHSNSummary([FromQuery] int month, [FromQuery] int year)
    {
        try
        {
            if (month < 1 || month > 12)
            {
                return BadRequest("Month must be between 1 and 12");
            }

            if (year < 2000 || year > DateTime.Now.Year + 1)
            {
                return BadRequest("Invalid year");
            }

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var summary = await gstr1Service.GenerateHSNSummary(null, startDate, endDate);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating HSN Summary for {Month}/{Year}", month, year);
            return StatusCode(500, "An error occurred while generating the report");
        }
    }

    /// <summary>
    /// Export GSTR-1 report as JSON
    /// </summary>
    [HttpGet("gstr1/export/json")]
    public async Task<ActionResult> ExportGSTR1Json([FromQuery] int month, [FromQuery] int year)
    {
        try
        {
            var report = await gstr1Service.GenerateGSTR1ReportAsync(month, year);
            var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            var fileName = $"GSTR1_{month:D2}_{year}.json";
            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error exporting GSTR-1 as JSON for {Month}/{Year}", month, year);
            return StatusCode(500, "An error occurred while exporting the report");
        }
    }

    /// <summary>
    /// Export GSTR-3B report as JSON
    /// </summary>
    [HttpGet("gstr3b/export/json")]
    public async Task<ActionResult> ExportGSTR3BJson([FromQuery] int month, [FromQuery] int year)
    {
        try
        {
            var report = await gstr3BService.GenerateGSTR3BReportAsync(month, year);
            var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            var fileName = $"GSTR3B_{month:D2}_{year}.json";
            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error exporting GSTR-3B as JSON for {Month}/{Year}", month, year);
            return StatusCode(500, "An error occurred while exporting the report");
        }
    }
}
