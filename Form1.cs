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
        private ConcurrentDictionary<string, Image> CachedIcons = new ConcurrentDictionary<string, Image>();
        private List<string> noIconCacheExtensions = new List<string>() { ".lnk", ".url", ".exe" };
        private List<FileSystemWatcher> FileSystemWatchers = new List<FileSystemWatcher>();
        private DateTime? LastChangeNotification = null;
        private bool FW_HaveChanges = false;
        public frmOptions()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void frmOptions_Load(object sender, EventArgs e)
        {
            lstFolders.Items.Clear();
            foreach (var folder in Properties.Settings.Default.Folders)
            {
                lstFolders.Items.Add(folder);
            }
            createFileWatchers();
            backgroundWorker1.RunWorkerAsync();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy && FW_HaveChanges == true)
            {
                timer1.Stop();
                FW_HaveChanges = false;
                backgroundWorker1.RunWorkerAsync();
            }
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            timer1.Start();
        }

        private void createFileWatchers()
        {
            FileSystemWatchers.ForEach(x => x.Dispose());
            FileSystemWatchers.Clear();
            foreach (var folder in Properties.Settings.Default.Folders)
            {
                if (System.IO.Directory.Exists(folder))
                {
                    var fw = new FileSystemWatcher()
                    {
                        Path = folder,
                        IncludeSubdirectories = true,
                        SynchronizingObject = this,
                        EnableRaisingEvents = true
                    };
                    if (folder.ToLower().EndsWith("portableapps"))
                    {
                        fw.Filter = "*.exe";
                    }
                    fw.Created += FW_Changed;
                    fw.Deleted += FW_Changed;
                    fw.Renamed += FW_Changed;
                    fw.Error += FW_Error;
                    FileSystemWatchers.Add(fw);
                }
            }
        }

        #region Menu building

        //BackgroundWorker job
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            mnuRightClick.Items.Clear();

            var folders = new List<string>();
            foreach (var folder in Properties.Settings.Default.Folders)
            {
                folders.Add(folder);
            }
            folders.Sort();

            foreach (var folder in folders)
            {
                if (System.IO.Directory.Exists(folder))
                {
                    var menu = MakeFolderMenuItem(folder);
                    MakeTree(folder, menu);
                    mnuRightClick.Items.Add(menu);
                }
            }
        }

        private void FW_Error(object sender, ErrorEventArgs e)
        {
            InvokeShowFWError(e);
        }
        private delegate void ShowFWErrorDelegate(ErrorEventArgs e);
        private void InvokeShowFWError(ErrorEventArgs e)
        {
            _ = Invoke(new ShowFWErrorDelegate(ShowFWError), e);
        }
        private void ShowFWError(ErrorEventArgs e)
        {
            new ToastContentBuilder().AddText("Error").AddText(e.GetException().Message).Show();
        }

        private void FW_Changed(object sender, FileSystemEventArgs e)
        {
            FW_HaveChanges = true;
        }

        private void MakeTree(string folder, ToolStripMenuItem parentMenu)
        {
            if (System.IO.Directory.Exists(folder))
            {
                var subFolders = System.IO.Directory.GetDirectories(folder);
                var files = System.IO.Directory.GetFiles(folder);
                if (folder.ToLower().EndsWith("portableapps"))
                {
                    files = files.Where(x => Path.GetExtension(x) == ".exe").ToArray();
                    subFolders = subFolders.Where(x => System.IO.Directory.GetFiles(x).Where(y => Path.GetExtension(y) == ".exe").Any()).ToArray();
                    foreach (var subFolder in subFolders)
                    {
                        var subFiles = System.IO.Directory.GetFiles(subFolder, "*.exe");
                        if (subFiles.Count() > 1)
                        {
                            var menu = MakeFolderMenuItem(subFolder);
                            foreach (var subFile in subFiles)
                            {
                                var menuItem = MakeFileMenuItem(subFile);
                                menu.DropDownItems.Add(menuItem);
                            }
                            parentMenu.DropDownItems.Add(menu);
                        }
                        else if (subFiles.Count() == 1)
                        {
                            var menuItem = MakeFileMenuItem(subFiles[0]);
                            parentMenu.DropDownItems.Add(menuItem);
                        }
                    }

                    foreach (var file in files)
                    {
                        var menu = MakeFileMenuItem(file);
                        parentMenu.DropDownItems.Add(menu);
                    }
                }
                else
                {
                    foreach (var subFolder in subFolders)
                    {
                        var menu = MakeFolderMenuItem(subFolder);
                        MakeTree(subFolder, menu);
                        parentMenu.DropDownItems.Add(menu);
                    }

                    foreach (var file in files)
                    {
                        var menu = MakeFileMenuItem(file);
                        parentMenu.DropDownItems.Add(menu);
                    }
                }
            }
        }

        private ToolStripMenuItem MakeFolderMenuItem(string folder)
        {
            var menu = new ToolStripMenuItem();
            menu.Text = System.IO.Path.GetFileName(folder);
            menu.Tag = folder;
            menu.DoubleClickEnabled = true;
            menu.DoubleClick += new EventHandler(this.folderMenu_DoubleClick);
            menu.MouseDown += new MouseEventHandler(this.folderMenu_Click);
            menu.Image = icoNotify.Icon.ToBitmap();
            return menu;
        }

        private ToolStripMenuItem MakeFileMenuItem(string file)
        {
            var menu = new ToolStripMenuItem();
            var ext = System.IO.Path.GetExtension(file);
            if (noIconCacheExtensions.Contains(ext))
                menu.Text = System.IO.Path.GetFileNameWithoutExtension(file);
            else
                menu.Text = System.IO.Path.GetFileName(file);
            menu.Tag = file;
            menu.MouseDown += new MouseEventHandler(this.fileMenu_Click);
            menu.Image = getIconFromFilename(file);
            return menu;
        }

        private void folderMenu_DoubleClick(object sender, EventArgs e)
        {
            var tag = (string)((ToolStripMenuItem)sender).Tag;
            if (!System.IO.Directory.Exists(tag))
            {
                new ToastContentBuilder().AddText($"Directory {tag} does not exist.").AddText("Please reload").Show();
                return;
            }

            ProcessStartInfo psi = new ProcessStartInfo(tag);
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        private void folderMenu_Click(object sender, MouseEventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            var tag = (string)(menuItem.Tag);
            if (!System.IO.Directory.Exists(tag))
            {
                new ToastContentBuilder().AddText($"Directory {tag} does not exist.").AddText("Please reload").Show();
                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                ShellContextMenu ctxMnu = new ShellContextMenu();
                DirectoryInfo[] arrFI = new DirectoryInfo[1];
                arrFI[0] = new DirectoryInfo(tag);
                ToolStrip parent = menuItem.GetCurrentParent();
                int y = 0;
                foreach (ToolStripItem item in parent.Items)
                {
                    if (item == menuItem)
                        break;
                    if (item.Visible == true)
                        y += item.Height;
                }
                var point = parent.PointToScreen(new Point(-4, y));
                ctxMnu.ShowContextMenu(arrFI, point);
            }
        }

        private void fileMenu_Click(object sender, MouseEventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            var tag = (string)(menuItem.Tag);
            if (!System.IO.File.Exists(tag))
            {
                new ToastContentBuilder().AddText($"File {tag} does not exist.").AddText("Please reload").Show();
                return;
            }
            if (e.Button == MouseButtons.Left)
            {
                ProcessStartInfo psi = new ProcessStartInfo(tag);
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            else if (e.Button == MouseButtons.Right)
            {
                ShellContextMenu ctxMnu = new ShellContextMenu();
                FileInfo[] arrFI = new FileInfo[1];
                arrFI[0] = new FileInfo(tag);
                ToolStrip parent = menuItem.GetCurrentParent();
                int y = 0;
                foreach (ToolStripItem item in parent.Items)
                {
                    if (item == menuItem)
                        break;
                    if (item.Visible == true)
                        y += item.Height;
                }
                var point = parent.PointToScreen(new Point(-4, y));
                ctxMnu.ShowContextMenu(arrFI, point);
            }
        }

        #endregion

        #region Configuration form on double click
        private void icoNotify_DoubleClick(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                this.Show();
                this.BringToFront();
            }
            else
            {
                new ToastContentBuilder().AddText("Loading...").AddText("Please wait").Show();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var fldWin = new FolderBrowserDialog();
            if (fldWin.ShowDialog() == DialogResult.OK)
            {
                if (!lstFolders.Items.Contains(fldWin.SelectedPath))
                {
                    lstFolders.Items.Add(fldWin.SelectedPath);
                }
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lstFolders.SelectedItems.Count == 1)
            {
                lstFolders.Items.Remove(lstFolders.SelectedItems[0]);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var folders = new System.Collections.Specialized.StringCollection();
            foreach (var folder in lstFolders.Items)
            {
                folders.Add((string)folder);
            }
            Properties.Settings.Default.Folders = folders;
            Properties.Settings.Default.Save();
            createFileWatchers();
            FW_HaveChanges = true;
            this.Hide();
        }

        private void frmOptions_Shown(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region System

        [DllImport("shell32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("user32")]
        public static extern int DestroyIcon(IntPtr hIcon);

        public const uint SHGFI_ICON = 0x000000100;
        public const uint SHGFI_SMALLICON = 0x000000001;
        public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

        public Image getIconFromFilename(string fName)
        {
            if (System.IO.Path.HasExtension(fName) == false)
                return null;

            var ext = System.IO.Path.GetExtension(fName);
            if (!noIconCacheExtensions.Contains(ext) && CachedIcons.ContainsKey(ext))
                return CachedIcons[ext];

            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(fName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_USEFILEATTRIBUTES | SHGFI_ICON | SHGFI_SMALLICON);
            Icon myIcon = (System.Drawing.Icon)(System.Drawing.Icon.FromHandle(shinfo.hIcon).Clone());
            DestroyIcon(shinfo.hIcon);
            var result = myIcon.ToBitmap();
            CachedIcons.TryAdd(ext, result);
            return result;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        #endregion

    }
}
