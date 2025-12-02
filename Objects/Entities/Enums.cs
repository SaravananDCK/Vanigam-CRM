using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Vanigam.CRM.Objects.Entities
{
    public enum LeadStatus { New, Contacted, Qualified, Converted, Lost }
    public enum JobStatus { Pending, Assigned, Scheduled, [Display(Description = "In-Progress")] InProgress, [Display(Description = "On-Hold")] OnHold, Completed, Cancelled, Closed }
    public enum AppointmentStatus { Scheduled, [Display(Description = "Checked-In")] CheckedIn, [Display(Description = "In-Progress")] InProgress, Completed, Missed, Cancelled }
    public enum AssignmentStatus { Pending, Accepted, Rejected, [Display(Description = "En-Route")] EnRoute, Arrived, Paused, Finished }
    public enum TechnicianStatus { Available, Busy, Offline, [Display(Description = "On-Leave")] OnLeave }
    public enum QuoteStatus { Draft, Sent, Accepted, Converted, Rejected, Expired }
    public enum InvoiceStatus { Draft,Posted, Sent, Paid, [Display(Description = "Partially-Paid")] PartiallyPaid, Overdue, Cancelled }
    public enum PaymentStatus { Successful, Failed, Pending, Refunded }
    public enum PaymentMethod { Cash, [Display(Description = "Bank Transfer")] BankTransfer, Cheque, Card, UPI, Wallet, [Display(Description = "Net Banking")] NetBanking, Other }
    public enum AssetStatus { Active, [Display(Description = "In-Repair")] InRepair, Decommissioned, [Display(Description = "Under Warranty")] UnderWarranty }
    public enum Priority { Low, Normal, High, Critical }
    public enum OpportunityStage { Prospecting, Qualified, Proposal, Negotiation, [Display(Description = "Closed Won")] ClosedWon, [Display(Description = "Closed Lost")] ClosedLost }
    public enum ActivityStatus { NotStarted, [Display(Description = "In-Progress")] InProgress, Pending, Completed, Cancelled }
    public enum ActivityType { Call, Email, Meeting, Task, Note, [Display(Description = "Lead Conversion")] LeadConversion, [Display(Description = "Customer Conversion")] CustomerConversion }
    public enum ContactType { Individual, Company }
    public enum CustomerType { Individual, Company }
    public enum CustomerStatus { Active, Inactive, Prospect, Suspended }
    public enum ContactStatus { Active, Inactive }
    public enum ItemType { Product, [Display(Description = "Inventory Item")] InventoryItem, [Display(Description = "Service Item")] ServiceItem }
    public enum AccountType { [Display(Description = "Ledger Account")] LedgerAccount, Customer, Vendor, [Display(Description = "Bank Account")] BankAccount }
    public enum VoucherType { Quote, Invoice, [Display(Description = "Purchase Order")] PurchaseOrder, [Display(Description = "Purchase Invoice")] PurchaseInvoice, Job, Payment }
    public enum VoucherLineType { [Display(Description = "Quote Line")] QuoteLine, [Display(Description = "Invoice Line")] InvoiceLine, [Display(Description = "Purchase Order Line")] PurchaseOrderLine, [Display(Description = "Purchase Invoice Line")] PurchaseInvoiceLine, [Display(Description = "Material Usage Line")] MaterialUsageLine }
    public enum EntryType { Debit, Credit }
    public enum DiscountType { Percentage, Amount }
    public enum AccountNature { Asset = 1, Liability = 2, Income = 3, Expense = 4 }

    // Contract Coverage Enums
    public enum ContractCoverageType { [Display(Description = "Full Coverage")] FullCoverage, [Display(Description = "Partial Coverage")] PartialCoverage, [Display(Description = "Labor Only")] LaborOnly }
    public enum CoverageRuleType { Free, Chargeable, [Display(Description = "Partially Chargeable")] PartiallyChargeable }

    // Contract Duration Enum
    public enum ContractDuration {
        Monthly = 1,
        Quarterly = 3,
        [Display(Description = "Semi Annual")]
        SemiAnnual = 6,
        Annual = 12,
        Biennial = 24,
        Triennial = 36
    }
    // Contract Type Enum (TPH Discriminator)
    public enum ContractType
    {
        Warranty,        // Free manufacturer/seller warranty
        AMC,             // Annual Maintenance Contract (paid)
        Guarantee,       // Extended guarantee period
        [Display(Description = "Service Contract")]
        ServiceContract  // General service agreement
    }
}
