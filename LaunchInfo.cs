using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrayFolderMenu
{
    public class LaunchInfo
    {
        public enum ItemKind
        {
            FilePath,          // absolute path (exe/html/etc.)
            Url,               // http/https
            UriScheme,         // steam:// etc.
            KnownFolderPath,   // {GUID}\sub\path
            AppsFolderItem     // AUMID or Shell ID (no slashes, not a path)
        }

        public ItemKind Kind { get; set; }
        public string Path { get; set; }
    }
}
