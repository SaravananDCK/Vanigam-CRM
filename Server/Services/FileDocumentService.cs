using System.Linq.Dynamic.Core;
using System.Text;
using Alexinea.FastMember;
using DevExpress.DataAccess.ObjectBinding;
using DevExpress.Pdf;
using DevExpress.Xpo;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Vanigam.CRM.Objects.Services;
using Vanigam.CRM.Server.Services;

namespace Vanigam.CRM.Server.Services;

public class FileDocumentService(
    VanigamAccountingDbContext context,
    ILogger<BaseService<FileDocument>> logger)
    : BaseService<FileDocument>(context, logger)
{

    public override DbSet<FileDocument> GetDbSet()
    {
        return Context.FileDocuments;
    }

    public async Task<IQueryable<FileDocument>> GetFileContent(Guid oid)
    {
        var item = GetDbSet().Where(f => f.Oid == oid);
        return item.Select(f => new FileDocument()
        {
            Oid = f.Oid,
            LedgerAccountId = f.LedgerAccountId,
            LedgerAccount = f.LedgerAccount,
            TenantId = f.TenantId,
            CategoryId = f.CategoryId,
            Category = f.Category,
            FileName = f.FileName,
            FileSizeStr = f.FileSizeStr,
            FileSize = f.FileSize,
            FileType = f.FileType,
            Content = f.Content,
        });
    }
}
