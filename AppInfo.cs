using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrayFolderMenu
{
    public sealed class AppInfo
    {
        public string Name { get; init; }
        public string AppUserModelId { get; init; }
        public LaunchInfo LaunchInfo { get; set; }
    }
}
