using Radzen;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Client.Helpers
{
    /// <summary>
    /// Helper class to provide consistent BadgeStyle for status enums across the application
    /// </summary>
    public static class BadgeHelper
    {
        #region LeadStatus
        public static BadgeStyle GetBadgeStyle(LeadStatus status)
        {
            return status switch
            {
                LeadStatus.New => BadgeStyle.Info,
                LeadStatus.Contacted => BadgeStyle.Primary,
                LeadStatus.Qualified => BadgeStyle.Success,
                LeadStatus.Converted => BadgeStyle.Success,
                LeadStatus.Lost => BadgeStyle.Danger,
                _ => BadgeStyle.Secondary
            };
        }
        #endregion

        #region JobStatus
        public static BadgeStyle GetBadgeStyle(JobStatus status)
        {
            return status switch
            {
                JobStatus.Pending => BadgeStyle.Warning,
                JobStatus.Assigned => BadgeStyle.Info,
                JobStatus.Scheduled => BadgeStyle.Primary,
                JobStatus.InProgress => BadgeStyle.Info,
                JobStatus.OnHold => BadgeStyle.Warning,
                JobStatus.Completed => BadgeStyle.Success,
                JobStatus.Cancelled => BadgeStyle.Danger,
                JobStatus.Closed => BadgeStyle.Secondary,
                _ => BadgeStyle.Light
            };
        }
        #endregion

        #region AppointmentStatus
        public static BadgeStyle GetBadgeStyle(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Scheduled => BadgeStyle.Primary,
                AppointmentStatus.CheckedIn => BadgeStyle.Info,
                AppointmentStatus.InProgress => BadgeStyle.Warning,
                AppointmentStatus.Completed => BadgeStyle.Success,
                AppointmentStatus.Missed => BadgeStyle.Danger,
                AppointmentStatus.Cancelled => BadgeStyle.Secondary,
                _ => BadgeStyle.Light
            };
        }
        #endregion

        #region AssignmentStatus
        public static BadgeStyle GetBadgeStyle(AssignmentStatus status)
        {
            return status switch
            {
                AssignmentStatus.Pending => BadgeStyle.Warning,
                AssignmentStatus.Accepted => BadgeStyle.Success,
                AssignmentStatus.Rejected => BadgeStyle.Danger,
                AssignmentStatus.EnRoute => BadgeStyle.Info,
                AssignmentStatus.Arrived => BadgeStyle.Primary,
                AssignmentStatus.Paused => BadgeStyle.Warning,
                AssignmentStatus.Finished => BadgeStyle.Success,
                _ => BadgeStyle.Light
            };
        }
        #endregion

        #region TechnicianStatus
        public static BadgeStyle GetBadgeStyle(TechnicianStatus status)
        {
            return status switch
            {
                TechnicianStatus.Available => BadgeStyle.Success,
                TechnicianStatus.Busy => BadgeStyle.Warning,
                TechnicianStatus.Offline => BadgeStyle.Secondary,
                TechnicianStatus.OnLeave => BadgeStyle.Info,
                _ => BadgeStyle.Light
            };
        }
        #endregion

        #region QuoteStatus
        public static BadgeStyle GetBadgeStyle(QuoteStatus status)
        {
            return status switch
            {
                QuoteStatus.Draft => BadgeStyle.Secondary,
                QuoteStatus.Sent => BadgeStyle.Info,
                QuoteStatus.Accepted => BadgeStyle.Success,
                QuoteStatus.Rejected => BadgeStyle.Danger,
                QuoteStatus.Expired => BadgeStyle.Warning,
                _ => BadgeStyle.Light
            };
        }
        #endregion

        #region InvoiceStatus
        public static BadgeStyle GetBadgeStyle(InvoiceStatus status)
        {
            return status switch
            {
                InvoiceStatus.Draft => BadgeStyle.Secondary,
                InvoiceStatus.Sent => BadgeStyle.Info,
                InvoiceStatus.Paid => BadgeStyle.Success,
                InvoiceStatus.PartiallyPaid => BadgeStyle.Warning,
                InvoiceStatus.Overdue => BadgeStyle.Danger,
                InvoiceStatus.Cancelled => BadgeStyle.Secondary,
                _ => BadgeStyle.Light
            };
        }
        #endregion

        #region PaymentStatus
        public static BadgeStyle GetBadgeStyle(PaymentStatus? status)
        {
            return status switch
            {
                PaymentStatus.Successful => BadgeStyle.Success,
                PaymentStatus.Failed => BadgeStyle.Danger,
                PaymentStatus.Pending => BadgeStyle.Warning,
                PaymentStatus.Refunded => BadgeStyle.Info,
                _ => BadgeStyle.Light
            };
        }
        #endregion
        #region PaymentMethod
        public static BadgeStyle GetBadgeStyle(PaymentMethod? status)
        {
            return status switch
            {
                PaymentMethod.Cash => BadgeStyle.Success,
                PaymentMethod.UPI => BadgeStyle.Info,
                PaymentMethod.Cheque => BadgeStyle.Secondary,
                PaymentMethod.BankTransfer => BadgeStyle.Warning,
                PaymentMethod.Card => BadgeStyle.Primary,
                PaymentMethod.NetBanking => BadgeStyle.Base,
                _ => BadgeStyle.Light
            };
        }
        #endregion

        #region AssetStatus
        public static BadgeStyle GetBadgeStyle(AssetStatus status)
        {
            return status switch
            {
                AssetStatus.Active => BadgeStyle.Success,
                AssetStatus.InRepair => BadgeStyle.Warning,
                AssetStatus.Decommissioned => BadgeStyle.Danger,
                AssetStatus.UnderWarranty => BadgeStyle.Info,
                _ => BadgeStyle.Light
            };
        }
        #endregion

        #region Priority
        public static BadgeStyle GetBadgeStyle(Priority priority)
        {
            return priority switch
            {
                Priority.Low => BadgeStyle.Info,
                Priority.Normal => BadgeStyle.Primary,
                Priority.High => BadgeStyle.Warning,
                Priority.Critical => BadgeStyle.Danger,
                _ => BadgeStyle.Light
            };
        }
        #endregion

        #region OpportunityStage
        public static BadgeStyle GetBadgeStyle(OpportunityStage stage)
        {
            return stage switch
            {
                OpportunityStage.Prospecting => BadgeStyle.Info,
                OpportunityStage.Qualified => BadgeStyle.Primary,
                OpportunityStage.Proposal => BadgeStyle.Warning,
                OpportunityStage.Negotiation => BadgeStyle.Warning,
                OpportunityStage.ClosedWon => BadgeStyle.Success,
                OpportunityStage.ClosedLost => BadgeStyle.Danger,
                _ => BadgeStyle.Light
            };
        }
        #endregion

        #region ActivityStatus
        public static BadgeStyle GetBadgeStyle(ActivityStatus status)
        {
            return status switch
            {
                ActivityStatus.NotStarted => BadgeStyle.Secondary,
                ActivityStatus.InProgress => BadgeStyle.Info,
                ActivityStatus.Pending => BadgeStyle.Warning,
                ActivityStatus.Completed => BadgeStyle.Success,
                ActivityStatus.Cancelled => BadgeStyle.Danger,
                _ => BadgeStyle.Light
            };
        }
        #endregion

        #region ActivityType
        public static BadgeStyle GetBadgeStyle(ActivityType type)
        {
            return type switch
            {
                ActivityType.Call => BadgeStyle.Info,
                ActivityType.Email => BadgeStyle.Primary,
                ActivityType.Meeting => BadgeStyle.Warning,
                ActivityType.Task => BadgeStyle.Secondary,
                ActivityType.Note => BadgeStyle.Light,
                ActivityType.LeadConversion => BadgeStyle.Success,
                ActivityType.CustomerConversion => BadgeStyle.Success,
                _ => BadgeStyle.Light
            };
        }
        #endregion
        #region AccountType
        public static BadgeStyle GetBadgeStyle(AccountType type)
        {
            return type switch
            {
                AccountType.LedgerAccount => BadgeStyle.Info,
                AccountType.BankAccount => BadgeStyle.Primary,
                AccountType.Customer => BadgeStyle.Success,
                AccountType.Vendor => BadgeStyle.Secondary,
                _ => BadgeStyle.Light
            };
        }
        #endregion

        #region CustomerStatus
        public static BadgeStyle GetBadgeStyle(CustomerStatus status)
        {
            return status switch
            {
                CustomerStatus.Active => BadgeStyle.Success,
                CustomerStatus.Inactive => BadgeStyle.Secondary,
                CustomerStatus.Prospect => BadgeStyle.Info,
                CustomerStatus.Suspended => BadgeStyle.Danger,
                _ => BadgeStyle.Light
            };
        }
        #endregion

        #region ContactStatus
        public static BadgeStyle GetBadgeStyle(ContactStatus status)
        {
            return status switch
            {
                ContactStatus.Active => BadgeStyle.Success,
                ContactStatus.Inactive => BadgeStyle.Secondary,
                _ => BadgeStyle.Light
            };
        }
        #endregion

        #region AccountNature
        public static BadgeStyle GetBadgeStyle(AccountNature nature)
        {
            return nature switch
            {
                AccountNature.Asset => BadgeStyle.Success,
                AccountNature.Liability => BadgeStyle.Warning,
                AccountNature.Income => BadgeStyle.Info,
                AccountNature.Expense => BadgeStyle.Danger,
                _ => BadgeStyle.Light
            };
        }
        #endregion

        #region Boolean Status (Active/Inactive)
        /// <summary>
        /// Generic helper for boolean status (IsActive, etc.)
        /// </summary>
        public static BadgeStyle GetBadgeStyleForBoolean(bool isActive)
        {
            return isActive ? BadgeStyle.Success : BadgeStyle.Danger;
        }
        #endregion

        #region PurchaseInvoiceStatus
        public static BadgeStyle GetBadgeStyle(PurchaseInvoiceStatus status)
        {
            return status switch
            {
                PurchaseInvoiceStatus.Draft => BadgeStyle.Secondary,
                PurchaseInvoiceStatus.Posted => BadgeStyle.Info,
                PurchaseInvoiceStatus.Received => BadgeStyle.Primary,
                PurchaseInvoiceStatus.Verified => BadgeStyle.Success,
                PurchaseInvoiceStatus.Paid => BadgeStyle.Success,
                PurchaseInvoiceStatus.PartiallyPaid => BadgeStyle.Warning,
                PurchaseInvoiceStatus.Overdue => BadgeStyle.Danger,
                PurchaseInvoiceStatus.Cancelled => BadgeStyle.Secondary,
                _ => BadgeStyle.Light
            };
        }
        #endregion
        #region PurchaseInvoiceStatus
        public static BadgeStyle GetBadgeStyle(PurchaseOrderStatus status)
        {
            return status switch
            {
                PurchaseOrderStatus.Draft => BadgeStyle.Secondary,
                PurchaseOrderStatus.Sent => BadgeStyle.Info,
                PurchaseOrderStatus.Confirmed => BadgeStyle.Success,
                PurchaseOrderStatus.PartiallyReceived => BadgeStyle.Warning,
                PurchaseOrderStatus.Received => BadgeStyle.Danger,
                PurchaseOrderStatus.Cancelled => BadgeStyle.Secondary,
                _ => BadgeStyle.Light
            };
        }
        #endregion
    }
}
