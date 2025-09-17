namespace Vanigam.CRM.Reports
{
    partial class frmNewReport
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dataLayoutControl1 = new DevExpress.XtraDataLayout.DataLayoutControl();
            btnSave = new DevExpress.XtraEditors.SimpleButton();
            cbxDbSet = new ComboBox();
            cbxCategory = new ComboBox();
            txtExpands = new TextBox();
            txtName = new TextBox();
            Root = new DevExpress.XtraLayout.LayoutControlGroup();
            layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            emptySpaceItem2 = new DevExpress.XtraLayout.EmptySpaceItem();
            layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)dataLayoutControl1).BeginInit();
            dataLayoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Root).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)emptySpaceItem1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)emptySpaceItem2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem5).BeginInit();
            SuspendLayout();
            // 
            // dataLayoutControl1
            // 
            dataLayoutControl1.Controls.Add(btnSave);
            dataLayoutControl1.Controls.Add(cbxDbSet);
            dataLayoutControl1.Controls.Add(cbxCategory);
            dataLayoutControl1.Controls.Add(txtExpands);
            dataLayoutControl1.Controls.Add(txtName);
            dataLayoutControl1.Dock = DockStyle.Fill;
            dataLayoutControl1.Location = new Point(0, 0);
            dataLayoutControl1.Name = "dataLayoutControl1";
            dataLayoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new Rectangle(1203, 243, 650, 400);
            dataLayoutControl1.Root = Root;
            dataLayoutControl1.Size = new Size(800, 450);
            dataLayoutControl1.TabIndex = 0;
            dataLayoutControl1.Text = "dataLayoutControl1";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(402, 110);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(386, 22);
            btnSave.StyleController = dataLayoutControl1;
            btnSave.TabIndex = 4;
            btnSave.Text = "Save";
            btnSave.Click += btnSave_Click;
            // 
            // cbxDbSet
            // 
            cbxDbSet.FormattingEnabled = true;
            cbxDbSet.Location = new Point(69, 61);
            cbxDbSet.Name = "cbxDbSet";
            cbxDbSet.Size = new Size(719, 23);
            cbxDbSet.TabIndex = 2;
            // 
            // cbxCategory
            // 
            cbxCategory.FormattingEnabled = true;
            cbxCategory.Location = new Point(69, 36);
            cbxCategory.Name = "cbxCategory";
            cbxCategory.Size = new Size(719, 23);
            cbxCategory.TabIndex = 2;
            // 
            // textBox2
            // 
            txtExpands.Location = new Point(69, 86);
            txtExpands.Name = "txtExpands";
            txtExpands.Size = new Size(719, 20);
            txtExpands.TabIndex = 3;
            // 
            // textBox1
            // 
            txtName.Location = new Point(69, 12);
            txtName.Name = "txtName";
            txtName.Size = new Size(719, 20);
            txtName.TabIndex = 0;
            // 
            // Root
            // 
            Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            Root.GroupBordersVisible = false;
            Root.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] { layoutControlItem1, emptySpaceItem1, layoutControlItem2, layoutControlItem3, emptySpaceItem2, layoutControlItem4, layoutControlItem5 });
            Root.Name = "Root";
            Root.Size = new Size(800, 450);
            Root.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            layoutControlItem1.Control = txtName;
            layoutControlItem1.Location = new Point(0, 0);
            layoutControlItem1.Name = "layoutControlItem1";
            layoutControlItem1.Size = new Size(780, 24);
            layoutControlItem1.Text = "Name";
            layoutControlItem1.TextSize = new Size(45, 13);
            // 
            // emptySpaceItem1
            // 
            emptySpaceItem1.AllowHotTrack = false;
            emptySpaceItem1.Location = new Point(0, 124);
            emptySpaceItem1.Name = "emptySpaceItem1";
            emptySpaceItem1.Size = new Size(780, 306);
            emptySpaceItem1.TextSize = new Size(0, 0);
            // 
            // layoutControlItem2
            // 
            layoutControlItem2.Control = cbxCategory;
            layoutControlItem2.Location = new Point(0, 24);
            layoutControlItem2.Name = "layoutControlItem2";
            layoutControlItem2.Size = new Size(780, 25);
            layoutControlItem2.Text = "Category";
            layoutControlItem2.TextSize = new Size(45, 13);
            // 
            // layoutControlItem3
            // 
            layoutControlItem3.Control = btnSave;
            layoutControlItem3.Location = new Point(390, 98);
            layoutControlItem3.Name = "layoutControlItem3";
            layoutControlItem3.Size = new Size(390, 26);
            layoutControlItem3.TextSize = new Size(0, 0);
            layoutControlItem3.TextVisible = false;
            // 
            // emptySpaceItem2
            // 
            emptySpaceItem2.AllowHotTrack = false;
            emptySpaceItem2.Location = new Point(0, 98);
            emptySpaceItem2.Name = "emptySpaceItem2";
            emptySpaceItem2.Size = new Size(390, 26);
            emptySpaceItem2.TextSize = new Size(0, 0);
            // 
            // layoutControlItem4
            // 
            layoutControlItem4.Control = txtExpands;
            layoutControlItem4.Location = new Point(0, 74);
            layoutControlItem4.Name = "layoutControlItem4";
            layoutControlItem4.Size = new Size(780, 24);
            layoutControlItem4.Text = "Expands";
            layoutControlItem4.TextSize = new Size(45, 13);
            // 
            // layoutControlItem5
            // 
            layoutControlItem5.Control = cbxDbSet;
            layoutControlItem5.Location = new Point(0, 49);
            layoutControlItem5.Name = "layoutControlItem5";
            layoutControlItem5.Size = new Size(780, 25);
            layoutControlItem5.Text = "DbSet";
            layoutControlItem5.TextSize = new Size(45, 13);
            // 
            // frmNewReport
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dataLayoutControl1);
            Name = "frmNewReport";
            Text = "frmNewReport";
            ((System.ComponentModel.ISupportInitialize)dataLayoutControl1).EndInit();
            dataLayoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)Root).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem1).EndInit();
            ((System.ComponentModel.ISupportInitialize)emptySpaceItem1).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem2).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem3).EndInit();
            ((System.ComponentModel.ISupportInitialize)emptySpaceItem2).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem4).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem5).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraDataLayout.DataLayoutControl dataLayoutControl1;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private ComboBox cbxCategory;
        private TextBox txtName;
        private DevExpress.XtraLayout.LayoutControlGroup Root;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem2;
        private ComboBox cbxDbSet;
        private TextBox txtExpands;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
    }
}
