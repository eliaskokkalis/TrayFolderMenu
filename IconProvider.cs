using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TrayFolderMenu.LaunchInfo;

namespace TrayFolderMenu
{
    public static class IconProvider
    {
        public static Image GetImage(LaunchInfo launchInfo)
        {
            switch (launchInfo.Kind)
            {
                case ItemKind.FilePath:
                    return ShellInterop.getIconFromFilename(launchInfo.Path);

                case ItemKind.KnownFolderPath:
                    var expanded = ShellInterop.ExpandKnownFolderPath(launchInfo.Path);
                    return ShellInterop.getIconFromFilename(expanded);

                case ItemKind.AppsFolderItem:
                    return ShellInterop.GetAppsFolderIcon(launchInfo.Path, 32);

                case ItemKind.Url:
                case ItemKind.UriScheme:
                    // Use handler executable’s icon (default browser for http/https, Steam for steam://, etc.)
                    var scheme = launchInfo.Path.Split(':')[0];
                    var handler = ShellInterop.GetHandlerExecutableForProtocol(scheme);
                    if (!string.IsNullOrEmpty(handler))
                        return ShellInterop.getIconFromFilename(handler);

                    // Fallback: generic link icon from shell32 (optional: use SHGetStockIconInfo)
                    return SystemIcons.Application.ToBitmap();

                default:
                    return null;
            }
        }
    }
}
