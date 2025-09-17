using System.ComponentModel.DataAnnotations;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class Notification : BaseClass
    {
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Message { get; set; }
        public Guid? RecipientUserId { get; set; }
        public bool Sent { get; set; } = false;
    }
}
