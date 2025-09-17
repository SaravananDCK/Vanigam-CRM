using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Attributes;
using Vanigam.CRM.Objects.Contracts;
using Vanigam.CRM.Objects.Enums;

namespace Vanigam.CRM.Objects.Entities;

[NonTenantObject]
public abstract class DocumentTemplate : BaseClass
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Required]
    public string Content { get; set; }

    public Guid? FileCategoryId { get; set; }

    [ForeignKey(nameof(FileCategoryId))]
    public FileCategory FileCategory { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; }

    [StringLength(500)]
    public string Expands { get; set; }

    [StringLength(100)]
    public string DbSet { get; set; }
    public Guid? PreviewOid { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TemplateTypes TemplateType { get; set; }
}


