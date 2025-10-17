
namespace TrayFolderMenu
{
    partial class frmOptions
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
            components = new System.ComponentModel.Container();
            btnAdd = new System.Windows.Forms.Button();
            btnRemove = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            btnOK = new System.Windows.Forms.Button();
            btnExit = new System.Windows.Forms.Button();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            grdFolders = new System.Windows.Forms.DataGridView();
            pathDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            showExeOnlyDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            folderConfigBindingSource = new System.Windows.Forms.BindingSource(components);
            chkShowApps = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)grdFolders).BeginInit();
            ((System.ComponentModel.ISupportInitialize)folderConfigBindingSource).BeginInit();
            SuspendLayout();
            // 
            // btnAdd
            // 
            btnAdd.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnAdd.Location = new System.Drawing.Point(488, 12);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new System.Drawing.Size(32, 32);
            btnAdd.TabIndex = 3;
            btnAdd.Text = "+";
            toolTip1.SetToolTip(btnAdd, "Add folder");
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnRemove
            // 
            btnRemove.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnRemove.Location = new System.Drawing.Point(488, 50);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new System.Drawing.Size(32, 32);
            btnRemove.TabIndex = 4;
            btnRemove.Text = "-";
            toolTip1.SetToolTip(btnRemove, "Remove folder");
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(450, 228);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(32, 32);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "X";
            toolTip1.SetToolTip(btnCancel, "Cancel changes and hide options");
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnOK
            // 
            btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOK.Location = new System.Drawing.Point(488, 228);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(32, 32);
            btnOK.TabIndex = 6;
            btnOK.Text = "✓";
            toolTip1.SetToolTip(btnOK, "Save changes and hide options");
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnExit
            // 
            btnExit.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnExit.Location = new System.Drawing.Point(12, 228);
            btnExit.Name = "btnExit";
            btnExit.Size = new System.Drawing.Size(122, 32);
            btnExit.TabIndex = 7;
            btnExit.Text = "Exit Application";
            toolTip1.SetToolTip(btnExit, "Exit application");
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += btnExit_Click;
            // 
            // toolTip1
            // 
            toolTip1.AutomaticDelay = 300;
            // 
            // grdFolders
            // 
            grdFolders.AllowUserToAddRows = false;
            grdFolders.AllowUserToDeleteRows = false;
            grdFolders.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            grdFolders.AutoGenerateColumns = false;
            grdFolders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grdFolders.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { pathDataGridViewTextBoxColumn, showExeOnlyDataGridViewCheckBoxColumn });
            grdFolders.DataSource = folderConfigBindingSource;
            grdFolders.Location = new System.Drawing.Point(12, 12);
            grdFolders.Name = "grdFolders";
            grdFolders.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            grdFolders.Size = new System.Drawing.Size(470, 185);
            grdFolders.TabIndex = 8;
            // 
            // pathDataGridViewTextBoxColumn
            // 
            pathDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            pathDataGridViewTextBoxColumn.DataPropertyName = "Path";
            pathDataGridViewTextBoxColumn.HeaderText = "Path";
            pathDataGridViewTextBoxColumn.Name = "pathDataGridViewTextBoxColumn";
            pathDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // showExeOnlyDataGridViewCheckBoxColumn
            // 
            showExeOnlyDataGridViewCheckBoxColumn.DataPropertyName = "ShowExeOnly";
            showExeOnlyDataGridViewCheckBoxColumn.HeaderText = "ShowExeOnly";
            showExeOnlyDataGridViewCheckBoxColumn.MinimumWidth = 100;
            showExeOnlyDataGridViewCheckBoxColumn.Name = "showExeOnlyDataGridViewCheckBoxColumn";
            showExeOnlyDataGridViewCheckBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // folderConfigBindingSource
            // 
            folderConfigBindingSource.DataSource = typeof(FolderConfig);
            // 
            // chkShowApps
            // 
            chkShowApps.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            chkShowApps.AutoSize = true;
            chkShowApps.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            chkShowApps.Location = new System.Drawing.Point(12, 203);
            chkShowApps.Name = "chkShowApps";
            chkShowApps.Size = new System.Drawing.Size(119, 19);
            chkShowApps.TabIndex = 9;
            chkShowApps.Text = "Show Apps Menu";
            chkShowApps.UseVisualStyleBackColor = true;
            // 
            // frmOptions
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(532, 272);
            ControlBox = false;
            Controls.Add(chkShowApps);
            Controls.Add(grdFolders);
            Controls.Add(btnExit);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            Controls.Add(btnRemove);
            Controls.Add(btnAdd);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Name = "frmOptions";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Tray Folder Menu Options";
            TopMost = true;
            Load += frmOptions_Load;
            ((System.ComponentModel.ISupportInitialize)grdFolders).EndInit();
            ((System.ComponentModel.ISupportInitialize)folderConfigBindingSource).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.DataGridView grdFolders;
        private System.Windows.Forms.BindingSource folderConfigBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn pathDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn showExeOnlyDataGridViewCheckBoxColumn;
        private System.Windows.Forms.CheckBox chkShowApps;
    }
}

