namespace Vanigam.CRM.Objects.Entities
{
    public enum LeadStatus { New, Contacted, Qualified, Converted, Lost }
    public enum JobStatus { Pending, Assigned, Scheduled, InProgress, OnHold, Completed, Cancelled, Closed }
    public enum AppointmentStatus { Scheduled, CheckedIn, InProgress, Completed, Missed, Cancelled }
    public enum AssignmentStatus { Pending, Accepted, Rejected, EnRoute, Arrived, Paused, Finished }
    public enum TechnicianStatus { Available, Busy, Offline, OnLeave }
    public enum QuoteStatus { Draft, Sent, Accepted, Rejected, Expired }
    public enum InvoiceStatus { Draft,Posted, Sent, Paid, PartiallyPaid, Overdue, Cancelled }
    public enum PaymentStatus { Successful, Failed, Pending, Refunded }
    public enum PaymentMethod { Cash, BankTransfer, Cheque, Card, UPI, Wallet, NetBanking, Other }
    public enum AssetStatus { Active, InRepair, Decommissioned, UnderWarranty }
    public enum Priority { Low, Normal, High, Critical }
    public enum OpportunityStage { Prospecting, Qualified, Proposal, Negotiation, ClosedWon, ClosedLost }
    public enum ActivityStatus { NotStarted, InProgress, Pending, Completed, Cancelled }
    public enum ActivityType { Call, Email, Meeting, Task, Note,LeadConversion, CustomerConversion }
    public enum ContactType { Individual, Company }
    public enum CustomerType { Individual, Company }
    public enum CustomerStatus { Active, Inactive, Prospect, Suspended }
    public enum ContactStatus { Active, Inactive }
    public enum ItemType { Product, InventoryItem, ServiceItem }
    public enum AccountType { LedgerAccount, Customer, Vendor, BankAccount }
    public enum VoucherType { Quote, Invoice, PurchaseOrder, PurchaseInvoice,Job,Payment }
    public enum VoucherLineType { QuoteLine, InvoiceLine, PurchaseOrderLine, PurchaseInvoiceLine, MaterialUsageLine }
    public enum EntryType { Debit, Credit }
    public enum AccountNature { Asset = 1, Liability = 2, Income = 3, Expense = 4 }

    // Contract Coverage Enums
    public enum ContractCoverageType { FullCoverage, PartialCoverage, LaborOnly }
    public enum CoverageRuleType { Free, Chargeable, PartiallyChargeable }

    // Contract Duration Enum
    public enum ContractDuration {
        Monthly = 1,
        Quarterly = 3,
        SemiAnnual = 6,
        Annual = 12,
        Biennial = 24,
        Triennial = 36
    }
}
