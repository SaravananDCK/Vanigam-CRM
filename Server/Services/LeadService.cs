using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using NodaTime;
using Vanigam.CRM.Objects.Helpers;

namespace Vanigam.CRM.Server.Services;

public class LeadService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<Lead>> logger)
    : BaseService<Lead>(context, logger)
{
    public override DbSet<Lead> GetDbSet()
    {
        return Context.Leads;
    }

    /// <summary>
    /// Converts a Lead to an Opportunity
    /// </summary>
    /// <param name="leadId">ID of the Lead to convert</param>
    /// <param name="opportunityTitle">Title for the new Opportunity</param>
    /// <param name="estimatedValue">Estimated value for the Opportunity</param>
    /// <param name="expectedCloseDate">Expected close date for the Opportunity</param>
    /// <returns>The created Opportunity</returns>
    public async Task<Opportunity> ConvertLeadToOpportunityAsync(Guid leadId, string opportunityTitle, decimal estimatedValue, DateTime expectedCloseDate)
    {
        Logger.LogInformation("Converting Lead {LeadId} to Opportunity", leadId);

        using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the lead
            var lead = await Context.Leads
                .FirstOrDefaultAsync(l => l.Oid == leadId);

            if (lead == null)
            {
                throw new InvalidOperationException($"Lead with ID {leadId} not found");
            }

            if (lead.Status == LeadStatus.Converted)
            {
                throw new InvalidOperationException("Lead has already been converted");
            }

            // Create opportunity from lead
            var opportunity = new Opportunity
            {
                Oid = Guid.NewGuid(),
                TenantId = lead.TenantId,
                LeadId = lead.Oid,
                Title = opportunityTitle,
                Description = $"Opportunity converted from lead: {lead.Name} ({lead.Organization ?? "Individual"})",
                EstimatedValue = estimatedValue,
                Probability = 75, // Default probability for qualified opportunities
                Source = lead.Source ?? "Lead Conversion",
                Stage = OpportunityStage.Qualified,
                ExpectedCloseDate = expectedCloseDate.ToDateTimeOffset(),
                Comments = $"Converted from lead on {DateTime.UtcNow:yyyy-MM-dd}. Original lead score: {lead.LeadScore}",
                CreatedByUserId = lead.CreatedByUserId,
                CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                UpdatedByUserId = lead.UpdatedByUserId,
                UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                IsNotDeleted = true
            };

            // Add opportunity to context
            Context.Opportunities.Add(opportunity);

            // Update lead status to converted
            lead.Status = LeadStatus.Converted;
            lead.UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();

            // Create an activity to track the conversion
            var activity = new Activity
            {
                Oid = Guid.NewGuid(),
                TenantId = lead.TenantId,
                LeadId = lead.Oid,
                Type = ActivityType.LeadConversion,
                Subject = $"Lead converted to Opportunity: {opportunityTitle}",
                Description = $"Lead '{lead.Name}' was successfully converted to opportunity '{opportunityTitle}' with estimated value {estimatedValue:C}",
                ActivityDate = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                Status = ActivityStatus.Completed ,
                CreatedByUserId = lead.UpdatedByUserId,
                CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                UpdatedByUserId = lead.UpdatedByUserId,
                UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                IsNotDeleted = true
            };

            Context.Activities.Add(activity);

            await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            Logger.LogInformation("Successfully converted Lead {LeadId} to Opportunity {OpportunityId}", leadId, opportunity.Oid);
            return opportunity;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Logger.LogError(ex, "Error converting Lead {LeadId} to Opportunity", leadId);
            throw;
        }
    }

    /// <summary>
    /// Converts an Opportunity to a Customer
    /// </summary>
    /// <param name="opportunityId">ID of the Opportunity to convert</param>
    /// <returns>The created Customer</returns>
    public async Task<Customer> ConvertOpportunityToCustomerAsync(Guid opportunityId)
    {
        Logger.LogInformation("Converting Opportunity {OpportunityId} to Customer", opportunityId);

        using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            // Get the opportunity with its lead
            var opportunity = await Context.Opportunities
                .Include(o => o.Lead)
                .FirstOrDefaultAsync(o => o.Oid == opportunityId);

            if (opportunity == null)
            {
                throw new InvalidOperationException($"Opportunity with ID {opportunityId} not found");
            }

            if (opportunity.Stage == OpportunityStage.ClosedWon)
            {
                // Check if customer already exists for this opportunity
                var existingCustomer = await Context.Customers
                    .FirstOrDefaultAsync(c => c.Name == (opportunity.Lead.Organization ?? opportunity.Lead.Name));

                if (existingCustomer != null)
                {
                    throw new InvalidOperationException("Customer already exists for this opportunity");
                }
            }

            if (opportunity.Stage == OpportunityStage.ClosedLost)
            {
                throw new InvalidOperationException("Cannot convert a lost opportunity to customer");
            }

            var lead = opportunity.Lead;

            // Create customer from lead information
            var customer = new Customer
            {
                Oid = Guid.NewGuid(),
                TenantId = lead.TenantId,
                Name = lead.Organization ?? lead.Name, // Use organization name if available, otherwise use lead name
                Industry = lead.Industry,
                Address = FormatAddress(lead),
                Email = lead.Email,
                Phone = lead.Phone,
                CreatedByUserId = lead.CreatedByUserId,
                CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                UpdatedByUserId = lead.UpdatedByUserId,
                UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                IsNotDeleted = true
            };

            // Add customer to context
            Context.Customers.Add(customer);

            // Create a primary contact from the lead if it's an individual
            if (!string.IsNullOrEmpty(lead.Name) && lead.Name != customer.Name)
            {
                var contact = new Contact
                {
                    Oid = Guid.NewGuid(),
                    TenantId = lead.TenantId,
                    CustomerId = customer.Oid,
                    Type = ContactType.Individual,
                    FirstName = ExtractFirstName(lead.Name),
                    LastName = ExtractLastName(lead.Name),
                    Email = lead.Email,
                    Phone = lead.Phone,
                    CreatedByUserId = lead.CreatedByUserId,
                    CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                    UpdatedByUserId = lead.UpdatedByUserId,
                    UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                    IsNotDeleted = true
                };

                Context.Contacts.Add(contact);
            }

            // Update opportunity stage to ClosedWon
            opportunity.Stage = OpportunityStage.ClosedWon;
            opportunity.UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset();

            // Create an activity to track the conversion
            var activity = new Activity
            {
                Oid = Guid.NewGuid(),
                TenantId = lead.TenantId,
                LeadId = lead.Oid,
                Type = ActivityType.CustomerConversion ,
                Subject = $"Opportunity converted to Customer: {customer.Name}",
                Description = $"Opportunity '{opportunity.Title}' was successfully converted to customer '{customer.Name}'",
                ActivityDate = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                Status = ActivityStatus.Completed,
                CreatedByUserId = lead.UpdatedByUserId,
                CreatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                UpdatedByUserId = lead.UpdatedByUserId,
                UpdatedAtUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset(),
                IsNotDeleted = true
            };

            Context.Activities.Add(activity);

            await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            Logger.LogInformation("Successfully converted Opportunity {OpportunityId} to Customer {CustomerId}", opportunityId, customer.Oid);
            return customer;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Logger.LogError(ex, "Error converting Opportunity {OpportunityId} to Customer", opportunityId);
            throw;
        }
    }

    private static string FormatAddress(Lead lead)
    {
        var addressParts = new List<string>();

        if (!string.IsNullOrEmpty(lead.Address))
            addressParts.Add(lead.Address);

        var cityStateZip = new List<string>();
        if (!string.IsNullOrEmpty(lead.City))
            cityStateZip.Add(lead.City);
        if (!string.IsNullOrEmpty(lead.State))
            cityStateZip.Add(lead.State);
        if (!string.IsNullOrEmpty(lead.PostalCode))
            cityStateZip.Add(lead.PostalCode);

        if (cityStateZip.Any())
            addressParts.Add(string.Join(", ", cityStateZip));

        if (!string.IsNullOrEmpty(lead.Country))
            addressParts.Add(lead.Country);

        return string.Join("\n", addressParts);
    }

    private static string ExtractFirstName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName))
            return string.Empty;

        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : string.Empty;
    }

    private static string ExtractLastName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName))
            return string.Empty;

        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty;
    }
}