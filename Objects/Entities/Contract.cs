using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    /// <summary>
    /// Base contract class using Table-Per-Hierarchy (TPH) inheritance
    /// Discriminator: ContractType enum
    /// </summary>
    [JsonDerivedType(typeof(WarrantyContract), nameof(ContractType.Warranty))]
    [JsonDerivedType(typeof(AmcContract), nameof(ContractType.AMC))]
    [JsonDerivedType(typeof(GuaranteeContract), nameof(ContractType.Guarantee))]
    [JsonDerivedType(typeof(ServiceContract), nameof(ContractType.ServiceContract))]
    public abstract class Contract : BaseClass
    {
        protected Contract() { }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public Guid? CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; } = null!;

        public DateTimeOffset? StartDate { get; set; }

        /// <summary>
        /// Duration of the contract in months
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContractDuration Duration { get; set; } = ContractDuration.Annual;

        /// <summary>
        /// Computed end date based on StartDate and Duration
        /// </summary>
        [NotMapped]
        public DateTimeOffset? EndDate
        {
            get
            {
                if (StartDate.HasValue)
                {
                    return StartDate.Value.AddMonths((int)Duration);
                }
                return null;
            }
        }

        [StringLength(5000)]
        public string? Terms { get; set; }

        /// <summary>
        /// Whether this contract is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Type of coverage provided by this contract
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContractCoverageType CoverageType { get; set; } = ContractCoverageType.PartialCoverage;

        /// <summary>
        /// Whether parts replacement is included in this contract
        /// </summary>
        public bool IncludesPartsReplacement { get; set; } = true;

        /// <summary>
        /// Overall parts replacement limit per year (null = unlimited)
        /// This is in addition to per-item limits defined in coverage rules
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PartsReplacementLimit { get; set; }

        /// <summary>
        /// Contract type discriminator for TPH inheritance
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContractType ContractType { get; set; }

        /// <summary>
        /// Link to original invoice (for warranty contracts auto-created from sales)
        /// </summary>
        public Guid? InvoiceId { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public Invoice? Invoice { get; set; }

        /// <summary>
        /// Contract value (for paid contracts like AMC - null for free warranty)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ContractValue { get; set; }

        /// <summary>
        /// Auto-renewal settings
        /// </summary>
        public bool AutoRenewEnabled { get; set; } = false;

        /// <summary>
        /// Number of days before expiry to send renewal notification
        /// </summary>
        public int? AutoRenewNoticeDays { get; set; } = 30;

        /// <summary>
        /// Parent contract reference (e.g., AMC contract references original warranty)
        /// </summary>
        public Guid? ParentContractId { get; set; }

        [ForeignKey(nameof(ParentContractId))]
        public Contract? ParentContract { get; set; }

        /// <summary>
        /// Child contracts (e.g., warranty that converted to AMC)
        /// </summary>
        public ICollection<Contract> ChildContracts { get; set; } = new List<Contract>();

        // Navigation properties
        public ICollection<RecurringJob> RecurringJobs { get; set; } = new List<RecurringJob>();
        public ICollection<ContractCoverageRule> CoverageRules { get; set; } = new List<ContractCoverageRule>();
    }

    /// <summary>
    /// Warranty Contract - Free manufacturer/seller warranty provided with product purchase
    /// </summary>
    public class WarrantyContract : Contract
    {
        public WarrantyContract()
        {
            ContractType = ContractType.Warranty;
            CoverageType = ContractCoverageType.FullCoverage;
            IncludesPartsReplacement = true;
            ContractValue = null; // Free warranty
            AutoRenewEnabled = false; // Warranties don't auto-renew
        }

        /// <summary>
        /// Warranty number/code provided by manufacturer
        /// </summary>
        [StringLength(100)]
        public string? WarrantyNumber { get; set; }

        /// <summary>
        /// Whether this warranty requires product registration
        /// </summary>
        public bool RequiresRegistration { get; set; } = false;

        /// <summary>
        /// Registration completion date
        /// </summary>
        public DateTimeOffset? RegistrationDate { get; set; }

        /// <summary>
        /// Whether this warranty is transferable to new owner
        /// </summary>
        public bool IsTransferable { get; set; } = false;
    }

    /// <summary>
    /// AMC (Annual Maintenance Contract) - Paid service contract for ongoing support
    /// </summary>
    public class AmcContract : Contract
    {
        public AmcContract()
        {
            ContractType = ContractType.AMC;
            CoverageType = ContractCoverageType.PartialCoverage;
            IncludesPartsReplacement = true;
            AutoRenewEnabled = true;
            AutoRenewNoticeDays = 30;
            Duration = ContractDuration.Annual;
        }

        /// <summary>
        /// AMC contract number
        /// </summary>
        [StringLength(100)]
        public string? AmcNumber { get; set; }

        /// <summary>
        /// Number of scheduled preventive maintenance visits per year
        /// </summary>
        public int? ScheduledVisitsPerYear { get; set; }

        /// <summary>
        /// Response time commitment in hours (e.g., 24 hours)
        /// </summary>
        public int? ResponseTimeHours { get; set; }

        /// <summary>
        /// Resolution time commitment in hours (e.g., 48 hours)
        /// </summary>
        public int? ResolutionTimeHours { get; set; }

        /// <summary>
        /// Whether 24/7 support is included
        /// </summary>
        public bool Includes24x7Support { get; set; } = false;

        /// <summary>
        /// Whether remote support is included
        /// </summary>
        public bool IncludesRemoteSupport { get; set; } = true;

        /// <summary>
        /// Whether on-site support is included
        /// </summary>
        public bool IncludesOnsiteSupport { get; set; } = true;

        /// <summary>
        /// Discount percentage for next renewal (loyalty discount)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? RenewalDiscountPercent { get; set; }
    }

    /// <summary>
    /// Guarantee Contract - Extended guarantee period beyond standard warranty
    /// </summary>
    public class GuaranteeContract : Contract
    {
        public GuaranteeContract()
        {
            ContractType = ContractType.Guarantee;
            CoverageType = ContractCoverageType.FullCoverage;
            IncludesPartsReplacement = true;
            AutoRenewEnabled = false;
        }

        /// <summary>
        /// Guarantee certificate number
        /// </summary>
        [StringLength(100)]
        public string? GuaranteeNumber { get; set; }

        /// <summary>
        /// Whether this is a money-back guarantee
        /// </summary>
        public bool IsMoneyBackGuarantee { get; set; } = false;

        /// <summary>
        /// Specific conditions or exclusions for the guarantee
        /// </summary>
        [StringLength(2000)]
        public string? GuaranteeConditions { get; set; }
    }

    /// <summary>
    /// Service Contract - General service agreement (not product-specific)
    /// </summary>
    public class ServiceContract : Contract
    {
        public ServiceContract()
        {
            ContractType = ContractType.ServiceContract;
            CoverageType = ContractCoverageType.PartialCoverage;
            IncludesPartsReplacement = false; // Service only by default
            AutoRenewEnabled = true;
            AutoRenewNoticeDays = 30;
        }

        /// <summary>
        /// Service contract number
        /// </summary>
        [StringLength(100)]
        public string? ServiceContractNumber { get; set; }

        /// <summary>
        /// Type of services covered (e.g., "IT Support", "Network Maintenance")
        /// </summary>
        [StringLength(500)]
        public string? ServiceType { get; set; }

        /// <summary>
        /// Number of service hours included per month/year
        /// </summary>
        public int? IncludedServiceHours { get; set; }

        /// <summary>
        /// Hourly rate for additional hours beyond included
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AdditionalHourlyRate { get; set; }
    }
}
