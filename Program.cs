using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static TrayFolderMenu.LaunchInfo;

namespace TrayFolderMenu
{
    static class Program
    {
        static NotifyIcon trayIcon = new NotifyIcon();
        static ContextMenuStrip contextMenu = new ContextMenuStrip();
        static BackgroundWorker backgroundWorker = new BackgroundWorker();
        static Timer timer = new Timer();
        static List<FileSystemWatcher> FileSystemWatchers = new List<FileSystemWatcher>();
        static bool FW_HaveChanges = false;

        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            trayIcon.Text = "Right click for folder menu, double click for options.";
            trayIcon.Icon = Properties.Resources.icoFolder;
            trayIcon.Visible = true;
            contextMenu.Size = new Size(61, 4);
            trayIcon.ContextMenuStrip = contextMenu;
            trayIcon.DoubleClick += TrayIcon_DoubleClick;

            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            timer.Interval = 10000;
            timer.Tick += Timer_Tick;

            createFileWatchers();
            backgroundWorker.RunWorkerAsync();

            Control.CheckForIllegalCrossThreadCalls = false;

            Application.Run();
        }

        private static void createFileWatchers()
        {
            FileSystemWatchers.ForEach(x =>
            {
                x.Created -= FW_Changed;
                x.Deleted -= FW_Changed;
                x.Renamed -= FW_Changed;
                x.Error -= FW_Error;
                x.Dispose();
            });
            FileSystemWatchers.Clear();
            foreach (var folder in Properties.Settings.Default.Folders)
            {
                var folderConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<FolderConfig>(folder);
                if (Directory.Exists(folderConfig.Path))
                {
                    var fw = new FileSystemWatcher()
                    {
                        Path = folderConfig.Path,
                        IncludeSubdirectories = true,
                        EnableRaisingEvents = true
                    };
                    if (folderConfig.ShowExeOnly == true)
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

        private static void FW_Error(object sender, ErrorEventArgs e)
        {
            new ToastContentBuilder().AddText("Error").AddText(e.GetException().Message).Show();
        }

        private static void FW_Changed(object sender, FileSystemEventArgs e)
        {
            FW_HaveChanges = true;
        }

        private static void Timer_Tick(object sender, EventArgs e)
        {
            if (!backgroundWorker.IsBusy && FW_HaveChanges == true)
            {
                timer.Stop();
                FW_HaveChanges = false;
                backgroundWorker.RunWorkerAsync();
            }
        }

        private static void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Properties.Settings.Default.ShowApps == true && e.Error == null)
            {
                var appMenu = new ToolStripMenuItem();
                appMenu.Text = "Apps";
                appMenu.DoubleClickEnabled = false;
                appMenu.Image = Properties.Resources.icoFolder.ToBitmap();

                var apps = (List<AppInfo>)e.Result;
                apps = apps.OrderBy(x =>x.LaunchInfo.Kind.ToString() + " - " + x.Name).ToList();
                foreach (var app in apps)
                {
                    var menuItem = new ToolStripMenuItem();
                    menuItem.Text = app.LaunchInfo.Kind.ToString() + " - " + app.Name;
                    menuItem.Tag = app.LaunchInfo;
                    menuItem.MouseDown += FileMenu_MouseDown;
                    menuItem.Image = IconProvider.GetImage(app.LaunchInfo);
                    appMenu.DropDownItems.Add(menuItem);
                }
                contextMenu.Items.Add(appMenu);
            }

            timer.Start();
        }

        private static void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            contextMenu.Items.Clear();

            var folders = new List<string>();
            foreach (var folder in Properties.Settings.Default.Folders)
            {
                var folderConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<FolderConfig>(folder);
                folders.Add(folderConfig.Path);
            }
            folders.Sort();

            foreach (var folder in folders)
            {
                if (Directory.Exists(folder))
                {
                    var menu = MakeFolderMenuItem(folder);
                    MakeTree(folder, menu);
                    contextMenu.Items.Add(menu);
                }
            }

            if (Properties.Settings.Default.ShowApps == true)
            {
                // This is an MTA thread. We hop to a dedicated STA and wait.
                e.Result = AppsFolderReader.EnumerateAppsOnSta();
            }
        }

        private static ToolStripMenuItem MakeFolderMenuItem(string folder)
        {
            var folderMenu = new ToolStripMenuItem();
            folderMenu.Text = Path.GetFileName(folder);
            folderMenu.Tag = new LaunchInfo() { Kind = LaunchInfo.ItemKind.FilePath, Path = folder };
            folderMenu.DoubleClickEnabled = true;
            folderMenu.DoubleClick += FolderMenu_DoubleClick;
            folderMenu.MouseDown += FolderMenu_MouseDown;
            folderMenu.Image = Properties.Resources.icoFolder.ToBitmap();
            return folderMenu;
        }

        private static void MakeTree(string folder, ToolStripMenuItem parentMenu)
        {
            if (Directory.Exists(folder))
            {
                var subFolders = Directory.GetDirectories(folder);
                var files = Directory.GetFiles(folder);
                if (folder.ToLower().EndsWith("portableapps"))
                {
                    files = files.Where(x => Path.GetExtension(x) == ".exe").ToArray();
                    subFolders = subFolders.Where(x => Directory.GetFiles(x).Where(y => Path.GetExtension(y) == ".exe").Any()).ToArray();
                    foreach (var subFolder in subFolders)
                    {
                        var subFiles = Directory.GetFiles(subFolder, "*.exe");
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

        private static ToolStripMenuItem MakeFileMenuItem(string file)
        {
            var fileMenu = new ToolStripMenuItem();
            var ext = Path.GetExtension(file);
            if (ShellInterop.noIconCacheExtensions.Contains(ext))
                fileMenu.Text = Path.GetFileNameWithoutExtension(file);
            else
                fileMenu.Text = Path.GetFileName(file);
            fileMenu.Tag = new LaunchInfo() { Kind = LaunchInfo.ItemKind.FilePath, Path = file };
            fileMenu.MouseDown += FileMenu_MouseDown;
            fileMenu.Image = IconProvider.GetImage((LaunchInfo)fileMenu.Tag);
            return fileMenu;
        }

        private static void FileMenu_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                var menuItem = (ToolStripMenuItem)sender;
                var tag = (LaunchInfo)(menuItem.Tag);
                if (tag.Kind == LaunchInfo.ItemKind.FilePath && !File.Exists(tag.Path))
                {
                    new ToastContentBuilder().AddText($"File {tag.Path} does not exist.").AddText("Please reload").Show();
                    return;
                }
                if (e.Button == MouseButtons.Left)
                {
                    switch (tag.Kind)
                    {
                        case ItemKind.FilePath:
                            Process.Start(new ProcessStartInfo(tag.Path) { UseShellExecute = true });
                            break;

                        case ItemKind.Url:
                        case ItemKind.UriScheme:
                            Process.Start(new ProcessStartInfo(tag.Path) { UseShellExecute = true });
                            break;

                        case ItemKind.KnownFolderPath:
                            Process.Start(new ProcessStartInfo(ShellInterop.ExpandKnownFolderPath(tag.Path)) { UseShellExecute = true });
                            break;

                        case ItemKind.AppsFolderItem:
                            // Works for UWP (…!App), unpackaged AUMIDs (e.g., Microsoft.VisualStudioCode),
                            // “AutoGenerated” IDs, Chrome/MSEdge IDs, etc.
                            Process.Start(new ProcessStartInfo("explorer.exe", $"shell:AppsFolder\\{tag.Path}")
                            { UseShellExecute = true });
                            break;
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {
                    if (tag.Kind == ItemKind.FilePath)
                    {
                        ShellContextMenu ctxMnu = new ShellContextMenu();
                        FileInfo[] arrFI = new FileInfo[1];
                        arrFI[0] = new FileInfo(tag.Path);
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
            }
            catch (Exception ex)
            {
                new ToastContentBuilder().AddText(ex.Message).Show();
            }
        }

        private static void FolderMenu_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                var menuItem = (ToolStripMenuItem)sender;
                var tag = (LaunchInfo)(menuItem.Tag);
                if (!Directory.Exists(tag.Path))
                {
                    new ToastContentBuilder().AddText($"Directory {tag.Path} does not exist.").AddText("Please reload").Show();
                    return;
                }

                if (e.Button == MouseButtons.Right)
                {
                    ShellContextMenu ctxMnu = new ShellContextMenu();
                    DirectoryInfo[] arrFI = new DirectoryInfo[1];
                    arrFI[0] = new DirectoryInfo(tag.Path);
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
            catch (Exception ex)
            {
                new ToastContentBuilder().AddText(ex.Message).Show();
            }
        }

        private static void FolderMenu_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                var tag = (LaunchInfo)((ToolStripMenuItem)sender).Tag;
                if (!Directory.Exists(tag.Path))
                {
                    new ToastContentBuilder().AddText($"Directory {tag.Path} does not exist.").AddText("Please reload").Show();
                    return;
                }

                ProcessStartInfo psi = new ProcessStartInfo(tag.Path);
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                new ToastContentBuilder().AddText(ex.Message).Show();
            }
        }

        private static void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            if (!backgroundWorker.IsBusy)
            {
                var frm = new frmOptions();
                frm.BringToFront();
                var res = frm.ShowDialog();
                if (res == DialogResult.OK)
                {
                    createFileWatchers();
                    FW_HaveChanges = true;
                }
            }
            else
            {
                new ToastContentBuilder().AddText("Loading...").AddText("Please wait").Show();
            }
        }
    }
}
