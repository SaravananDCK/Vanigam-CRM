
namespace Vanigam.CRM.Reports
{
    partial class frmReports
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            gridControl1 = new DevExpress.XtraGrid.GridControl();
            gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            gridColumnOid = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumnName = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumnExpands = new DevExpress.XtraGrid.Columns.GridColumn();
            panelControl1 = new DevExpress.XtraEditors.PanelControl();
            btnNewReport = new DevExpress.XtraEditors.SimpleButton();
            btnShowDesigner = new Button();
            ((System.ComponentModel.ISupportInitialize)gridControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelControl1).BeginInit();
            panelControl1.SuspendLayout();
            SuspendLayout();
            // 
            // gridControl1
            // 
            gridControl1.Dock = DockStyle.Fill;
            gridControl1.EmbeddedNavigator.Margin = new Padding(3, 2, 3, 2);
            gridControl1.Location = new Point(0, 48);
            gridControl1.MainView = gridView1;
            gridControl1.Margin = new Padding(3, 2, 3, 2);
            gridControl1.Name = "gridControl1";
            gridControl1.Size = new Size(700, 290);
            gridControl1.TabIndex = 0;
            gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView1 });
            gridControl1.MouseDoubleClick += gridControl1_MouseDoubleClick;
            // 
            // gridView1
            // 
            gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { gridColumnOid, gridColumnName, gridColumnExpands });
            gridView1.DetailHeight = 262;
            gridView1.GridControl = gridControl1;
            gridView1.Name = "gridView1";
            gridView1.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseUp;
            gridView1.OptionsEditForm.PopupEditFormWidth = 700;
            gridView1.ShowingEditor += gridView1_ShowingEditor;
            // 
            // gridColumnOid
            // 
            gridColumnOid.Caption = "Oid";
            gridColumnOid.FieldName = "Oid";
            gridColumnOid.Name = "gridColumnOid";
            gridColumnOid.Visible = true;
            gridColumnOid.VisibleIndex = 0;
            // 
            // gridColumnName
            // 
            gridColumnName.Caption = "Name";
            gridColumnName.FieldName = "Name";
            gridColumnName.Name = "gridColumnName";
            gridColumnName.Visible = true;
            gridColumnName.VisibleIndex = 1;
            // 
            // gridColumnExpands
            // 
            gridColumnExpands.Caption = "Expands";
            gridColumnExpands.FieldName = "Expands";
            gridColumnExpands.Name = "gridColumnExpands";
            gridColumnExpands.Visible = true;
            gridColumnExpands.VisibleIndex = 2;
            // 
            // panelControl1
            // 
            panelControl1.Controls.Add(btnShowDesigner);
            panelControl1.Controls.Add(btnNewReport);
            panelControl1.Dock = DockStyle.Top;
            panelControl1.Location = new Point(0, 0);
            panelControl1.Name = "panelControl1";
            panelControl1.Size = new Size(700, 48);
            panelControl1.TabIndex = 1;
            // 
            // btnNewReport
            // 
            btnNewReport.Location = new Point(12, 12);
            btnNewReport.Name = "btnNewReport";
            btnNewReport.Size = new Size(75, 23);
            btnNewReport.TabIndex = 0;
            btnNewReport.Text = "New Report";
            btnNewReport.Click += btnNewReport_Click;
            // 
            // btnShowDesigner
            // 
            btnShowDesigner.Location = new Point(105, 12);
            btnShowDesigner.Name = "btnShowDesigner";
            btnShowDesigner.Size = new Size(130, 23);
            btnShowDesigner.TabIndex = 1;
            btnShowDesigner.Text = "Show Designer";
            btnShowDesigner.UseVisualStyleBackColor = true;
            btnShowDesigner.Click += btnShowDesigner_Click;
            // 
            // frmReports
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(700, 338);
            Controls.Add(gridControl1);
            Controls.Add(panelControl1);
            Margin = new Padding(3, 2, 3, 2);
            Name = "frmReports";
            Text = "Form1";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)gridControl1).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelControl1).EndInit();
            panelControl1.ResumeLayout(false);
            ResumeLayout(false);
        }



        #endregion

        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnOid;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnName;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.SimpleButton btnNewReport;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnExpands;
        private Button btnShowDesigner;
    }
}

