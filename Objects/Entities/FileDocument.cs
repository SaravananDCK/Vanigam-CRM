using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Vanigam.CRM.Objects.Contracts;
using Vanigam.CRM.Objects.Enums;

namespace Vanigam.CRM.Objects.Entities;
public class FileDocument: BaseClass
{

    public string FileName { get; set; }
    public long? FileSize { get; set; }
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string FileSizeStr { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FileTypes FileType { get; set; }
    
    [ForeignKey(nameof(CategoryId))]
    public FileCategory Category { get; set; }
    public Guid? CategoryId { get; set; }
    //[NotMapped]
    public string Content { get; set; }

    public void RefreshFileType()
    {
        FileSize = System.Text.Encoding.ASCII.GetByteCount(Content);
        if (FileName.EndsWith(".pdf"))
        {
            FileType = FileTypes.PDF;
        }
        else if (FileName.EndsWith(".doc"))
        {
            FileType = FileTypes.DOC;
        }
        else if (FileName.EndsWith(".docx"))
        {
            FileType = FileTypes.DOCX;
        }
        else if (FileName.EndsWith(".jpg"))
        {
            FileType = FileTypes.JPG;
        }
        else if (FileName.EndsWith(".jpeg"))
        {
            FileType = FileTypes.JPEG;
        }
        else if (FileName.EndsWith(".png"))
        {
            FileType = FileTypes.PNG;
        }
        else if (FileName.EndsWith(".gif"))
        {
            FileType = FileTypes.GIF;
        }
    }
    [JsonIgnore, NotMapped]
    public string AuditInfo => FileName;
}
