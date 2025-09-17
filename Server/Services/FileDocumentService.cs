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
}
