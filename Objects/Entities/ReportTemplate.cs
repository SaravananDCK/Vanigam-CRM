using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vanigam.CRM.Objects.Contracts;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Objects.Entities;

public class ReportTemplate : DocumentTemplate
{

}
public class PdfTemplate : DocumentTemplate
{
    public ICollection<PdfField> Fields { get; set; } = new List<PdfField>();
    public ICollection<SignPdfField> SignFields { get; set; } = new List<SignPdfField>();
}
public class DocxTemplate : DocumentTemplate
{

}
public class DocxMacroTemplate : DocumentTemplate
{

}
public class PdfField : BaseClass
{
    [ForeignKey(nameof(Template))]
    public Guid? TemplateId { get; set; }
    public PdfTemplate Template { get; set; }

    [Required]
    [StringLength(100)]
    public string FieldName { get; set; }

    [Required]
    [StringLength(200)]
    public string MappedProperty { get; set; }
}
public class SignPdfField : BaseClass
{
    [ForeignKey(nameof(Template))]
    public Guid? TemplateId { get; set; }
    public PdfTemplate Template { get; set; }

    [Required]
    [StringLength(200)]
    public string MappedProperty { get; set; }
    public int Top { get; set; }
    public int Left { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public int? Page { get; set; }
}

