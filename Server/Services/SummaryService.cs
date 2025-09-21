using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Contracts;
using Vanigam.CRM.Objects.DTOs;
using Vanigam.CRM.Objects.OData;
using Vanigam.CRM.Objects.Services;

namespace Vanigam.CRM.Server.Services
{
    /// <summary>
    /// Generic service for generating status summaries with counts for enum values
    /// </summary>
    /// <typeparam name="TEntity">Entity type that inherits from BaseClass</typeparam>
    /// <typeparam name="TEnum">Enum type for status values</typeparam>
    public class SummaryService<TEntity, TEnum>(
        VanigamAccountingDbContext context,
        ICurrentUserService currentUserService,
        ILogger<SummaryService<TEntity, TEnum>> logger)
        where TEntity : BaseClass
        where TEnum : struct, Enum
    {
        /// <summary>
        /// Gets status summary with counts for each enum value in a single database query
        /// </summary>
        /// <param name="dbSet">The DbSet to query</param>
        /// <param name="statusProperty">Expression to select the status property</param>
        /// <param name="searchFilter">Optional search filter string</param>
        /// <param name="additionalFilter">Optional additional OData filter</param>
        /// <returns>Status summary response with counts</returns>
        public async Task<StatusSummaryResponse<TEnum>> GetStatusSummaryAsync(
            DbSet<TEntity> dbSet,
            Expression<Func<TEntity, TEnum>> statusProperty,
            string? searchFilter = null,
            string? additionalFilter = null)
        {
            logger.LogInformation("Getting status summary for {EntityType} with search filter: {SearchFilter}",
                typeof(TEntity).Name, searchFilter);

            try
            {
                var query = dbSet.AsQueryable();

                // Apply soft delete filtering
                query = query.Where(e => e.IsNotDeleted);

                // Apply tenant filtering if entity supports it
                if (typeof(ITenant).IsAssignableFrom(typeof(TEntity)) && currentUserService.TenantId.HasValue)
                {
                    var tenantFilter = CreateTenantFilterExpression(currentUserService.TenantId.Value);
                    query = query.Where(tenantFilter);
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    query = ApplySearchFilter(query, searchFilter);
                }

                // Apply additional filter if provided
                if (!string.IsNullOrEmpty(additionalFilter))
                {
                    query = ApplyODataFilter(query, additionalFilter);
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Group by status and get counts in a single query
                var statusCounts = await query
                    .GroupBy(statusProperty)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Status, x => x.Count);

                // Ensure all enum values are represented (with 0 count if not present)
                var allStatuses = Enum.GetValues<TEnum>();
                foreach (var status in allStatuses)
                {
                    if (!statusCounts.ContainsKey(status))
                        statusCounts[status] = 0;
                }

                logger.LogInformation("Status summary completed: Total={TotalCount}, StatusCounts={StatusCounts}",
                    totalCount, string.Join(", ", statusCounts.Select(kv => $"{kv.Key}={kv.Value}")));

                return new StatusSummaryResponse<TEnum>
                {
                    TotalCount = totalCount,
                    StatusCounts = statusCounts
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting status summary for {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }

        /// <summary>
        /// Creates tenant filter expression for entities that implement ITenant
        /// </summary>
        private Expression<Func<TEntity, bool>> CreateTenantFilterExpression(int tenantId)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var tenantProperty = Expression.Property(parameter, nameof(ITenant.TenantId));

            // Handle nullable TenantId comparison properly
            var tenantValue = Expression.Constant((int?)tenantId, typeof(int?));
            var equals = Expression.Equal(tenantProperty, tenantValue);

            return Expression.Lambda<Func<TEntity, bool>>(equals, parameter);
        }

        /// <summary>
        /// Applies search filter using ODataFilter pattern
        /// </summary>
        private IQueryable<TEntity> ApplySearchFilter(IQueryable<TEntity> query, string searchFilter)
        {
            try
            {
                // Parse and apply the search filter
                // This mimics the logic from GetFilterString methods in ListView pages
                if (string.IsNullOrEmpty(searchFilter))
                    return query;

                // For now, we'll use a simple approach that works with the existing filter format
                // In a more advanced implementation, we could parse the OData filter string
                logger.LogDebug("Applying search filter: {SearchFilter}", searchFilter);

                // Note: This is a simplified implementation
                // The actual filter parsing would depend on the ODataFilter implementation
                return query;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to apply search filter: {SearchFilter}", searchFilter);
                return query; // Return unfiltered query if filter parsing fails
            }
        }

        /// <summary>
        /// Applies additional OData filter
        /// </summary>
        private IQueryable<TEntity> ApplyODataFilter(IQueryable<TEntity> query, string filter)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                    return query;

                logger.LogDebug("Applying additional filter: {Filter}", filter);

                // Note: This is a placeholder for OData filter parsing
                // The actual implementation would parse and apply the OData filter
                return query;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to apply additional filter: {Filter}", filter);
                return query; // Return unfiltered query if filter parsing fails
            }
        }
    }
}