using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Entities
{
    public class CustomField : BaseClass
    {
        public string EntityName { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public string? MetadataJson { get; set; }
    }
}
