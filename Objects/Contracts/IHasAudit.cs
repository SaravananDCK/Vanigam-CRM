using System.ComponentModel.DataAnnotations.Schema;

namespace Vanigam.CRM.Objects.Contracts
{
    public interface IHasAudit
    {
        string? CreatedByUserId { get; set; }
        DateTimeOffset? CreatedAtUtc { get; set; }
        string? UpdatedByUserId { get; set; }
        DateTimeOffset? UpdatedAtUtc { get; set; }
        string? CreatedByUserName { get; set; }
        string? UpdatedByUserName { get; set; }
        string? CreatedAtString { get; set; }
        string? UpdatedAtString { get; set; }
    }
}

