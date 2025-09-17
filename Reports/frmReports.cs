using System;
using System.ComponentModel;
using System.Linq;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using Vanigam.CRM.Objects;
using Vanigam.CRM.Objects.Entities;
using Microsoft.EntityFrameworkCore;
using Vanigam.CRM.Objects.Entities;

namespace Vanigam.CRM.Reports
{
    public partial class frmReports : Form
    {
        private VanigamAccountingDbContext DbContext { get; set; }
        bool doubleClick = false;
        public frmReports(VanigamAccountingDbContext dbContext)
        {
            InitializeComponent();
            DbContext = dbContext;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var productsQuery = DbContext.ReportTemplates.ToList();
            gridControl1.DataSource = productsQuery;
        }

        private void btnNewReport_Click(object sender, EventArgs e)
        {
            new frmNewReport(DbContext,null).ShowDialog();
        }
        private void gridView1_ShowingEditor(object sender, CancelEventArgs e)
        {
            try
            {
                if (!doubleClick)
                    e.Cancel = true;
                else
                {
                    doubleClick = false;
                    new frmNewReport(DbContext, gridView1.GetFocusedRow() as ReportTemplate).ShowDialog();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.StackTrace);
            }

        }

        private void gridControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            GridControl grid = sender as GridControl;
            ColumnView view = grid.FocusedView as ColumnView;
            GridHitInfo hi = view.CalcHitInfo(e.Location) as GridHitInfo;
            if (hi.HitTest == GridHitTest.RowCell)
                doubleClick = true;
        }

        private void btnShowDesigner_Click(object sender, EventArgs e)
        {
            new frmReportDesigner(DbContext, gridView1.GetFocusedRow() as ReportTemplate).ShowDialog();
        }
    }
}

