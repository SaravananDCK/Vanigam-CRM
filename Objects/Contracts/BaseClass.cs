using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Contracts
{
    public interface ITenant
    {
        int? TenantId { get; set; }
    }
    
    public abstract class BaseClass : IHasId<Guid>, IHasAudit, IHasSoftDelete, ITenant,IETag
    {
        [NotMapped]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("@odata.etag")]
        public string ETag
        {
            get;
            set;
        } = string.Empty;
        public int? TenantId { get; set; }
        
        //IHasId
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Oid { get; set; }

        //IHasAudit
        public string? CreatedByUserId { get; set; }
        public DateTimeOffset? CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
        public string? UpdatedByUserId { get; set; }
        public DateTimeOffset? UpdatedAtUtc { get; set; }       
        public string? CreatedByUserName { get; set; }      
        public string? UpdatedByUserName { get; set; }       
        public string? CreatedAtString { get; set; }        
        public string? UpdatedAtString { get; set; }
        //IHasSoftDelete
        public bool IsNotDeleted { get; set; } = true;
                     
    }

    public interface IETag
    {
        string ETag { get; set; }
    }
}

