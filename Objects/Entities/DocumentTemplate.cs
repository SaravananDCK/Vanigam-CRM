using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Attributes;
using Vanigam.CRM.Objects.Contracts;
using Vanigam.CRM.Objects.Enums;

namespace Vanigam.CRM.Objects.Entities;

[NonTenantObject]
public abstract class DocumentTemplate : BaseClass
{
    public string Name { get; set; }
    public string Content { get; set; }
    [ForeignKey(nameof(FileCategory))]
    public Guid? FileCategoryId { get; set; }
    public FileCategory FileCategory { get; set; }

    public string FileName { get; set; }

    public string Expands { get; set; }
    public string DbSet { get; set; }
    public Guid? PreviewOid { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TemplateTypes TemplateType { get; set; }
}


