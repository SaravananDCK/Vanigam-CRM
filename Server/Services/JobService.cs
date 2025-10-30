using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Server.Services;

public class JobService(
    VanigamAccountingDbContext context,
    NumberSeriesService numberSeriesService,
    ILogger<BaseService<Job>> logger)
    : BaseService<Job>(context, logger)
{
    public override DbSet<Job> GetDbSet()
    {
        return Context.Jobs;
    }

    /// <summary>
    /// Bulk save job with materials. Handles create/update of job and its material usage.
    /// </summary>
    public async Task<Job> BulkSaveJobWithMaterials(JobBulkSaveDTO jobData)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            Job job;
            bool isUpdate = jobData.Oid.HasValue;

            if (isUpdate)
            {
                // Load existing job
                job = await Context.Jobs
                    .Include(j => j.VoucherLines)
                    .FirstOrDefaultAsync(j => j.Oid == jobData.Oid.Value);

                if (job == null)
                    throw new InvalidOperationException("Job not found");

                // Update job properties
                job.Number = jobData.Number;
                job.Title = jobData.Title;
                job.Description = jobData.Description;
                job.Status = jobData.Status;
                job.Priority = jobData.Priority;
                job.PartyId = jobData.PartyId;
                job.ContactId = jobData.ContactId;
                job.TotalAmount = jobData.TotalAmount;
                job.SubTotal = jobData.SubTotal;
                job.TaxAmount = jobData.TaxAmount;
                job.CGSTAmount = jobData.CGSTAmount;
                job.SGSTAmount = jobData.SGSTAmount;
                job.IGSTAmount = jobData.IGSTAmount;
                job.CessAmount = jobData.CessAmount;
                job.VoucherDate = jobData.VoucherDate;
                job.DiscountAmount = jobData.DiscountAmount;
                job.DiscountPercent = jobData.DiscountPercentage;
                job.DiscountType = jobData.DiscountPercentage > 0 ? DiscountType.Percentage : DiscountType.Amount;

                // Handle material usage
                await HandleMaterialUsage(job, jobData.Materials);

                Context.Jobs.Update(job);
                await Context.SaveChangesAsync();
            }
            else
            {
                var jobNumber = await numberSeriesService.GenerateNextNumber(nameof(Job), TenantId);
                // Create new job
                job = new Job
                {
                    Oid = Guid.NewGuid(),
                    Number = jobNumber,
                    Title = jobData.Title,
                    Description = jobData.Description,
                    Status = jobData.Status,
                    Priority = jobData.Priority,
                    PartyId = jobData.PartyId,
                    ContactId = jobData.ContactId,
                    TotalAmount = jobData.TotalAmount,
                    SubTotal = jobData.SubTotal,
                    TaxAmount = jobData.TaxAmount,
                    CGSTAmount = jobData.CGSTAmount,
                    SGSTAmount = jobData.SGSTAmount,
                    IGSTAmount = jobData.IGSTAmount,
                    CessAmount = jobData.CessAmount,
                    VoucherDate = jobData.VoucherDate,
                    DiscountAmount = jobData.DiscountAmount,
                    DiscountPercent = jobData.DiscountPercentage,
                    DiscountType = jobData.DiscountPercentage > 0 ? DiscountType.Percentage : DiscountType.Amount,
                    TenantId = TenantId
                };

                // Add material usage
                foreach (var materialDto in jobData.Materials.Where(m => !m.IsDeleted))
                {
                    var newMaterial = new MaterialUsage
                    {
                        Oid = Guid.NewGuid(),
                        VoucherId = job.Oid,
                        ItemId = materialDto.InventoryItemId,
                        Quantity = materialDto.Quantity,
                        UnitPrice = materialDto.UnitPrice,
                        DiscountAmount = materialDto.DiscountAmount,
                        TaxAmount = materialDto.TaxAmount ?? 0,
                        TaxCodeId = materialDto.TaxCodeId,
                        TenantId = TenantId
                    };

                    Context.MaterialUsages.Add(newMaterial);
                }

                Context.Jobs.Add(job);
                await Context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            // Reload job with materials
            var savedJob = await Context.Jobs
                .Include(j => j.VoucherLines)
                .FirstOrDefaultAsync(j => j.Oid == job.Oid);

            return savedJob!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Handles adding, updating, and deleting material usage
    /// </summary>
    private async Task HandleMaterialUsage(Job job, List<MaterialUsageDTO> materials)
    {
        // Remove deleted materials
        var deletedMaterialIds = materials
            .Where(m => m.IsDeleted && m.Oid.HasValue)
            .Select(m => m.Oid.Value)
            .ToList();

        if (deletedMaterialIds.Any())
        {
            var materialsToDelete = job.VoucherLines.OfType<MaterialUsage>().Where(m => deletedMaterialIds.Contains(m.Oid)).ToList();
            Context.MaterialUsages.RemoveRange(materialsToDelete);
        }

        // Add or update materials
        foreach (var materialDto in materials.Where(m => !m.IsDeleted))
        {
            if (materialDto.IsNew)
            {
                // Add new material
                var newMaterial = new MaterialUsage
                {
                    Oid = Guid.NewGuid(),
                    VoucherId = job.Oid,
                    ItemId = materialDto.InventoryItemId,
                    Quantity = materialDto.Quantity,
                    UnitPrice = materialDto.UnitPrice,
                    DiscountAmount = materialDto.DiscountAmount,
                    TaxAmount = materialDto.TaxAmount ?? 0,
                    TaxCodeId = materialDto.TaxCodeId,
                    TenantId = TenantId
                };

                Context.MaterialUsages.Add(newMaterial);
            }
            else if (materialDto.Oid.HasValue)
            {
                // Update existing material
                var existingMaterial = await Context.MaterialUsages
                    .FirstOrDefaultAsync(m => m.Oid == materialDto.Oid.Value);

                if (existingMaterial != null)
                {
                    existingMaterial.ItemId = materialDto.InventoryItemId;
                    existingMaterial.Quantity = materialDto.Quantity;
                    existingMaterial.UnitPrice = materialDto.UnitPrice;
                    existingMaterial.DiscountAmount = materialDto.DiscountAmount;
                    existingMaterial.TaxAmount = materialDto.TaxAmount ?? 0;
                    existingMaterial.TaxCodeId = materialDto.TaxCodeId;
                }
            }
        }
    }
}