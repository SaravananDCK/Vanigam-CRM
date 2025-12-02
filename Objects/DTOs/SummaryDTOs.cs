using System.Text.Json.Serialization;

namespace Vanigam.CRM.Objects.DTOs
{
    /// <summary>
    /// Response DTO for status summary containing counts for each enum value
    /// </summary>
    /// <typeparam name="TEnum">The enum type for status values</typeparam>
    public class StatusSummaryResponse<TEnum> where TEnum : Enum
    {
        /// <summary>
        /// Total count of all records (equivalent to "All" filter)
        /// </summary>
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        /// <summary>
        /// Dictionary containing count for each enum status value
        /// </summary>
        [JsonPropertyName("statusCounts")]
        public Dictionary<TEnum, int> StatusCounts { get; set; } = new();

        /// <summary>
        /// Helper property that includes null key for "All" option with total count
        /// Used by UI components that need nullable enum support
        /// </summary>
        [JsonIgnore]
        public Dictionary<TEnum?, int> StatusCountsNullable
        {
            get
            {
                var result = StatusCounts.ToDictionary(kv => (TEnum?)kv.Key, kv => kv.Value);
                result[default(TEnum?)] = TotalCount;
                return result;
            }
        }
    }

    /// <summary>
    /// Request DTO for status summary operations
    /// </summary>
    public class StatusSummaryRequest
    {
        /// <summary>
        /// Search filter string (from search box functionality)
        /// </summary>
        public string? SearchFilter { get; set; }

        /// <summary>
        /// Additional OData filter string for extra filtering
        /// </summary>
        public string? AdditionalFilter { get; set; }
    }

    /// <summary>
    /// Request DTO for converting a Lead to an Opportunity
    /// </summary>
    public class ConvertLeadToOpportunityRequest
    {
        /// <summary>
        /// The ID of the Lead to convert
        /// </summary>
        public Guid LeadId { get; set; }

        /// <summary>
        /// Title for the new Opportunity
        /// </summary>
        public string OpportunityTitle { get; set; } = string.Empty;

        /// <summary>
        /// Estimated value for the Opportunity
        /// </summary>
        public decimal EstimatedValue { get; set; }

        /// <summary>
        /// Expected close date for the Opportunity (must be UTC)
        /// </summary>
        public DateTimeOffset ExpectedCloseDate { get; set; }
    }

    /// <summary>
    /// Request DTO for converting an Opportunity to a Customer
    /// </summary>
    public class ConvertOpportunityToCustomerRequest
    {
        /// <summary>
        /// The ID of the Opportunity to convert
        /// </summary>
        public Guid OpportunityId { get; set; }
    }
}