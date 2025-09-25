namespace Vanigam.CRM.Objects.Entities
{
    public enum LeadStatus { New, Contacted, Qualified, Converted, Lost }
    public enum JobStatus { Pending, Assigned, Scheduled, InProgress, OnHold, Completed, Cancelled, Closed }
    public enum AppointmentStatus { Scheduled, CheckedIn, InProgress, Completed, Missed, Cancelled }
    public enum AssignmentStatus { Pending, Accepted, Rejected, EnRoute, Arrived, Paused, Finished }
    public enum TechnicianStatus { Available, Busy, Offline, OnLeave }
    public enum QuoteStatus { Draft, Sent, Accepted, Rejected, Expired }
    public enum InvoiceStatus { Draft, Sent, Paid, PartiallyPaid, Overdue, Cancelled }
    public enum PaymentStatus { Successful, Failed, Pending, Refunded }
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
}
