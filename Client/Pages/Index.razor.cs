using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.DTOs;

namespace Vanigam.CRM.Client.Pages
{
    public partial class Index
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected TooltipService TooltipService { get; set; }

        [Inject]
        protected ContextMenuService ContextMenuService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        protected SecurityService Security { get; set; }

        // Summary Data Properties
        private int TotalLeads { get; set; }
        private int TotalOpportunities { get; set; }
        private int TotalCustomers { get; set; }
        private decimal TotalRevenue { get; set; }

        // Chart Data Properties
        private List<StatusChartData>? LeadStatusData { get; set; }
        private List<StatusChartData>? JobStatusData { get; set; }
        private List<StatusChartData>? InvoiceStatusData { get; set; }
        private List<MonthlyData>? MonthlyRevenueData { get; set; }

        // Recent Data Properties
        private List<Lead>? RecentLeads { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadDashboardData();
        }

        private async Task LoadDashboardData()
        {
            try
            {
                // Load all data in parallel for better performance
                var leadSummaryTask = LoadLeadSummaryData();
                var opportunitySummaryTask = LoadOpportunitySummaryData();
                var jobSummaryTask = LoadJobSummaryData();
                var customerSummaryTask = LoadCustomerSummaryData();
                var revenueSummaryTask = LoadRevenueSummaryData();
                var invoiceSummaryTask = LoadInvoiceSummaryData();
                var recentLeadsTask = LoadRecentLeads();

                await Task.WhenAll(
                    leadSummaryTask,
                    opportunitySummaryTask,
                    jobSummaryTask,
                    customerSummaryTask,
                    revenueSummaryTask,
                    invoiceSummaryTask,
                    recentLeadsTask
                );

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
        }

        private async Task LoadLeadSummaryData()
        {
            try
            {
                // Get lead status summary using the same pattern as Leads.razor.cs
                var request = new StatusSummaryRequest();
                var leadSummary = await LeadApiService.GetStatusSummaryAsync(request);

                TotalLeads = leadSummary.TotalCount;

                // Create chart data for lead status distribution
                LeadStatusData = new List<StatusChartData>();
                foreach (LeadStatus status in Enum.GetValues<LeadStatus>())
                {
                    var count = leadSummary.StatusCounts.GetValueOrDefault(status, 0);
                    if (count > 0)
                    {
                        LeadStatusData.Add(new StatusChartData
                        {
                            Label = status.ToString(),
                            Value = count,
                            Color = GetLeadStatusColor(status)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading lead summary: {ex.Message}");
                TotalLeads = 0;
                LeadStatusData = new List<StatusChartData>();
            }
        }

        private async Task LoadOpportunitySummaryData()
        {
            try
            {
                var opportunityResult = await OpportunityApiService.Get(top: 1, count: true);
                TotalOpportunities = opportunityResult.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading opportunity summary: {ex.Message}");
                TotalOpportunities = 0;
            }
        }

        private async Task LoadJobSummaryData()
        {
            try
            {
                // Get total job count
                var jobResult = await JobApiService.Get(top: 1, count: true);
                var totalJobs = jobResult.Count;

                // Create sample job status data (since we don't have job status summary yet)
                JobStatusData = new List<StatusChartData>
                {
                    new() { Label = "New", Value = (int)(totalJobs * 0.2), Color = "#6c757d" },
                    new() { Label = "In Progress", Value = (int)(totalJobs * 0.4), Color = "#17a2b8" },
                    new() { Label = "Completed", Value = (int)(totalJobs * 0.3), Color = "#28a745" },
                    new() { Label = "Cancelled", Value = (int)(totalJobs * 0.1), Color = "#dc3545" }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading job summary: {ex.Message}");
                JobStatusData = new List<StatusChartData>();
            }
        }

        private async Task LoadCustomerSummaryData()
        {
            try
            {
                var customerResult = await CustomerApiService.Get(top: 1, count: true);
                TotalCustomers = customerResult.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading customer summary: {ex.Message}");
                TotalCustomers = 0;
            }
        }

        private async Task LoadRevenueSummaryData()
        {
            try
            {
                // Get total revenue from invoices
                var invoiceResult = await InvoiceApiService.Get(top: 1000); // Get more invoices for revenue calculation
                var invoices = invoiceResult.Value?.ToList() ?? new List<Invoice>();

                TotalRevenue = invoices.Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.PartiallyPaid)
                                      .Sum(i => i.TotalAmount);

                // Generate monthly revenue data for the chart (last 12 months)
                MonthlyRevenueData = new List<MonthlyData>();
                var currentDate = DateTime.Now;

                for (int i = 11; i >= 0; i--)
                {
                    var monthDate = currentDate.AddMonths(-i);
                    var monthRevenue = invoices
                        .Where(inv => inv.CreatedAtUtc?.Year == monthDate.Year &&
                                     inv.CreatedAtUtc?.Month == monthDate.Month &&
                                     (inv.Status == InvoiceStatus.Paid || inv.Status == InvoiceStatus.PartiallyPaid))
                        .Sum(inv => inv.TotalAmount);

                    MonthlyRevenueData.Add(new MonthlyData
                    {
                        Month = monthDate.ToString("MMM yyyy"),
                        Value = (double)monthRevenue
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading revenue summary: {ex.Message}");
                TotalRevenue = 0;
                MonthlyRevenueData = new List<MonthlyData>();
            }
        }

        private async Task LoadInvoiceSummaryData()
        {
            try
            {
                var invoiceResult = await InvoiceApiService.Get(top: 1000);
                var invoices = invoiceResult.Value?.ToList() ?? new List<Invoice>();

                // Create invoice status distribution
                InvoiceStatusData = new List<StatusChartData>();
                foreach (InvoiceStatus status in Enum.GetValues<InvoiceStatus>())
                {
                    var count = invoices.Count(i => i.Status == status);
                    if (count > 0)
                    {
                        InvoiceStatusData.Add(new StatusChartData
                        {
                            Label = status.ToString(),
                            Value = count,
                            Color = GetInvoiceStatusColor(status)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading invoice summary: {ex.Message}");
                InvoiceStatusData = new List<StatusChartData>();
            }
        }

        private async Task LoadRecentLeads()
        {
            try
            {
                var leadResult = await LeadApiService.Get(orderBy: "CreatedDate desc", top: 5);
                RecentLeads = leadResult.Value?.ToList() ?? new List<Lead>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading recent leads: {ex.Message}");
                RecentLeads = new List<Lead>();
            }
        }

        private string GetLeadStatusColor(LeadStatus status)
        {
            return status switch
            {
                LeadStatus.New => "#6c757d",        // Secondary gray
                LeadStatus.Contacted => "#17a2b8",  // Info blue
                LeadStatus.Qualified => "#ffc107",  // Warning yellow
                LeadStatus.Converted => "#28a745",  // Success green
                LeadStatus.Lost => "#dc3545",       // Danger red
                _ => "#6c757d"
            };
        }

        private string GetInvoiceStatusColor(InvoiceStatus status)
        {
            return status switch
            {
                InvoiceStatus.Draft => "#6c757d",
                InvoiceStatus.Sent => "#17a2b8",
                InvoiceStatus.Paid => "#28a745",
                InvoiceStatus.PartiallyPaid => "#ffc107",
                InvoiceStatus.Overdue => "#fd7e14",
                InvoiceStatus.Cancelled => "#dc3545",
                _ => "#6c757d"
            };
        }

        protected BadgeStyle GetLeadStatusBadgeStyle(LeadStatus status)
        {
            return status switch
            {
                LeadStatus.New => BadgeStyle.Secondary,
                LeadStatus.Contacted => BadgeStyle.Info,
                LeadStatus.Qualified => BadgeStyle.Warning,
                LeadStatus.Converted => BadgeStyle.Success,
                LeadStatus.Lost => BadgeStyle.Danger,
                _ => BadgeStyle.Light
            };
        }

        private void NavigateToPage(string url)
        {
            NavigationManager.NavigateTo(url);
        }

        //protected override async Task OnInitializedAsync()
        //{
        //    if (Security.User?.Name == ApplicationUser.TenantsAdmin)
        //        NavigationManager.NavigateTo("/application-tenants");
        //    else if (Security.UserType == UserType.Billing)
        //        NavigationManager.NavigateTo("/program-billings");
        //    else if (Security.UserType == UserType.Provider)
        //        NavigationManager.NavigateTo("/taskboard");
        //    else if (Security.IsInRole(ApplicationRole.PatientRoles))
        //        NavigationManager.NavigateTo("/patient-dashboard");
        //    else
        //        NavigationManager.NavigateTo("/dashboard");
        //}
    }

    // Chart data models
    public class StatusChartData
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class MonthlyData
    {
        public string Month { get; set; } = string.Empty;
        public double Value { get; set; }
    }
}

