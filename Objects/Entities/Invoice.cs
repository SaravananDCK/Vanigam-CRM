using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Invoice : Voucher
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        /// <summary>
        /// Total amount allocated from payments
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Remaining balance to be paid (TotalAmount - PaidAmount)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAmount { get; set; }

        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

        /// <summary>
        /// Payment allocations applied to this invoice
        /// </summary>
        public ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();

        /// <summary>
        /// Legacy payments collection (deprecated, use Allocations instead)
        /// </summary>
        [Obsolete("Use Allocations collection instead for better payment tracking")]
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        /// <summary>
        /// Calculates and updates the balance amount and invoice status based on paid amount
        /// </summary>
        public void UpdateBalance()
        {
            BalanceAmount = TotalAmount - PaidAmount;

            if (PaidAmount == 0)
            {
                Status = InvoiceStatus.Sent; // or Draft, depending on your workflow
            }
            else if (BalanceAmount <= 0)
            {
                Status = InvoiceStatus.Paid;
                BalanceAmount = 0;
            }
            else if (PaidAmount > 0 && BalanceAmount > 0)
            {
                Status = InvoiceStatus.PartiallyPaid;
            }

            // Check for overdue
            if (DueDate.HasValue && DueDate.Value < DateTimeOffset.UtcNow && BalanceAmount > 0)
            {
                Status = InvoiceStatus.Overdue;
            }
        }
    }
}
