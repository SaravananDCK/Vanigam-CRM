using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class UserSession : BaseClass
    {
        // Using Oid from BaseClass as primary key

        [Required]
        public Guid UserId { get; set; }

        [MaxLength(256)]
        public string? UserName { get; set; }

        [Required]
        public DateTimeOffset LoginTime { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? LogoutTime { get; set; }

        [Required]
        public DateTimeOffset LastActivityTime { get; set; } = DateTimeOffset.UtcNow;

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? DeviceInfo { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(1000)]
        public string? UserAgent { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [Column(TypeName = "decimal(10,8)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(11,8)")]
        public decimal? Longitude { get; set; }

        // Computed property for session duration
        [NotMapped]
        public int SessionDurationMinutes => 
            (int)(LogoutTime ?? DateTime.UtcNow).Subtract(LoginTime).TotalMinutes;

        [NotMapped]
        public TimeSpan SessionDuration => 
            (LogoutTime ?? DateTime.UtcNow).Subtract(LoginTime);

        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

    }
}
