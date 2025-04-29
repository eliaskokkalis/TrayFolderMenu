using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TrayFolderMenu
{
    public partial class frmOptions : Form
    {
        public frmOptions()
        {
            InitializeComponent();
        }

        private void frmOptions_Load(object sender, EventArgs e)
        {
            var folders = new List<FolderConfig>();
            foreach (var folder in Properties.Settings.Default.Folders)
            {
                var folderConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<FolderConfig>(folder);
                folders.Add(folderConfig);
            }
            folderConfigBindingSource.DataSource = folders;
        }


        #region Menu building


        //private void FW_Error(object sender, ErrorEventArgs e)
        //{
        //    InvokeShowFWError(e);
        //}
        //private delegate void ShowFWErrorDelegate(ErrorEventArgs e);
        //private void InvokeShowFWError(ErrorEventArgs e)
        //{
        //    _ = Invoke(new ShowFWErrorDelegate(ShowFWError), e);
        //}
        //private void ShowFWError(ErrorEventArgs e)
        //{
        //    new ToastContentBuilder().AddText("Error").AddText(e.GetException().Message).Show();
        //}


        #endregion


        private void btnAdd_Click(object sender, EventArgs e)
        {
            var fldWin = new FolderBrowserDialog();
            if (fldWin.ShowDialog() == DialogResult.OK)
            {
                if (!((List<FolderConfig>)folderConfigBindingSource.DataSource).Where(x => x.Path.ToLower() == fldWin.SelectedPath.ToLower()).Any())
                {
                    var folder = new FolderConfig() { Path = fldWin.SelectedPath, ShowExeOnly = false };
                    folderConfigBindingSource.Add(folder);
                }
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (grdFolders.SelectedRows.Count == 1)
            {
                grdFolders.Rows.Remove(grdFolders.SelectedRows[0]);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var folders = new System.Collections.Specialized.StringCollection();
            foreach (DataGridViewRow row in grdFolders.Rows)
            {
                var folderConfig = (FolderConfig)row.DataBoundItem;
                folders.Add(Newtonsoft.Json.JsonConvert.SerializeObject(folderConfig));
            }
            Properties.Settings.Default.Folders = folders;
            Properties.Settings.Default.Save();
            this.Hide();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
