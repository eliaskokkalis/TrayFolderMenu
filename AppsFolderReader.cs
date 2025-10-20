using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using static TrayFolderMenu.LaunchInfo;

namespace TrayFolderMenu
{
    public static class AppsFolderReader
    {
        static readonly Regex KnownFolderPrefix = new(@"^\{[0-9A-Fa-f\-]{36}\}\\?", RegexOptions.Compiled);

        public static List<AppInfo> EnumerateAppsOnSta()
        {
            var done = new ManualResetEventSlim(false);
            List<AppInfo> result = null;
            Exception error = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var list = new List<AppInfo>();

                    var shell = new Shell32.Shell();
                    var folder = shell.NameSpace("shell:AppsFolder");

                    foreach (Shell32.FolderItem2 item in folder.Items())
                    {
                        string name = item.Name;
                        string parsingPath = item.Path;
                        string aumid = item.ExtendedProperty("AppUserModelID") as string;

                        var appInfo = new AppInfo
                        {
                            Name = name,
                            AppUserModelId = aumid,
                            LaunchInfo = new LaunchInfo()
                            {
                                Kind = Classify(parsingPath),
                                Path = parsingPath
                            }
                        };

                        if (ShellContextMenu.SHGetIDListFromObject(item, out var pidl) == 0 && pidl != IntPtr.Zero)
                        {
                            appInfo.fi = ShellContextMenu.ILClone(pidl); // stable copy
                                                                         // store pidlClone somewhere (ListViewItem.Tag = pidlClone, or your model)
                                                                         // IMPORTANT: do NOT store `item` itself
                        }

                        list.Add(appInfo);
                    }

                    result = list;
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                finally
                {
                    done.Set();
                }
            });

            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA); // ⬅️ important
            thread.Start();

            done.Wait(); // we're on a worker thread, so blocking is fine here

            if (error != null) throw new InvalidOperationException("AppsFolder enumeration failed.", error);
            return result ?? new List<AppInfo>();
        }

        public static ItemKind Classify(string parsingPath)
        {
            if (string.IsNullOrWhiteSpace(parsingPath)) return ItemKind.AppsFolderItem;

            // URLs
            if (parsingPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                parsingPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return ItemKind.Url;

            // Custom URI scheme (has "something://")
            if (Uri.TryCreate(parsingPath, UriKind.Absolute, out var uri) && !uri.IsFile)
                return uri.Scheme is "http" or "https" ? ItemKind.Url : ItemKind.UriScheme;

            // Known-folder expansion pattern: {GUID}\...
            if (KnownFolderPrefix.IsMatch(parsingPath))
                return ItemKind.KnownFolderPath;

            // Absolute file path?
            if (Path.IsPathRooted(parsingPath))
                return ItemKind.FilePath;

            // Everything else: treat as AppsFolder item (AUMID/unpackaged ID)
            return ItemKind.AppsFolderItem;
        }

    }
}
