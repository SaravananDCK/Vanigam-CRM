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
        public int TotalCount { get; set; }

        /// <summary>
        /// Dictionary containing count for each enum status value
        /// </summary>
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
}