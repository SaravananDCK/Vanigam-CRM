using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cosmos.Reflection;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Reports
{
    public partial class frmNewReport : Form
    {
        //BindingSource AddressBindingSource { get; set; }= new BindingSource();
        private ReportTemplate ReportTemplate { get; set; } = new ReportTemplate();
        private VanigamAccountingDbContext DbContext { get; set; }
        public frmNewReport(VanigamAccountingDbContext dbContext, ReportTemplate reportTemplate)
        {
            InitializeComponent();
            if (reportTemplate != null)
            {
                ReportTemplate = reportTemplate;
                txtName.Text = ReportTemplate.Name;
                cbxDbSet.Text = ReportTemplate.DbSet;
                txtExpands.Text = ReportTemplate.Expands;
            }
            DbContext = dbContext;
            var properties = DbContext.GetProperties();
            foreach (var propertyInfo in properties)
            {
                if (typeof(System.Linq.IQueryable).IsAssignableFrom(propertyInfo.PropertyType))
                    cbxDbSet.Items.Add(propertyInfo.Name);
            }

            //AddressBindingSource.DataSource = typeof(ReportTemplate);
            //AddressBindingSource.AddNew();
            //dataLayoutControl1.DataSource = AddressBindingSource;
            //dataLayoutControl1.RetrieveFields();

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            ReportTemplate.Name = txtName.Text;
            ReportTemplate.DbSet = cbxDbSet.Text;
            ReportTemplate.Expands = txtExpands.Text;
            if (ReportTemplate.Oid==Guid.Empty)
            {
                DbContext?.ReportTemplates?.Add(ReportTemplate);
            }
            else
            {
                DbContext?.ReportTemplates?.Update(ReportTemplate);
            }
            DbContext?.SaveChanges();
            Close();
        }
    }
}

