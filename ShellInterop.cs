using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TrayFolderMenu
{
    public static class ShellInterop
    {
        static ConcurrentDictionary<string, Image> CachedIcons = new ConcurrentDictionary<string, Image>();
        public static readonly List<string> noIconCacheExtensions = new List<string>() { ".lnk", ".url", ".exe" };

        //-----------------------------------------------------------------------------
        [DllImport("shell32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("user32")]
        public static extern int DestroyIcon(IntPtr hIcon);

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr pvReserved);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        static extern int SHCreateItemFromParsingName(string pszPath, IntPtr pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IShellItemImageFactory ppv);

        [DllImport("shell32.dll")]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);

        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern uint AssocQueryString(ASSOCF flags, ASSOCSTR str, string pszAssoc, string pszExtra, StringBuilder pszOut, ref uint pcchOut);

        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(IntPtr hObject);

        [ComImport]
        [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IShellItemImageFactory
        {
            void GetImage(SIZE size, SIIGBF flags, out IntPtr phbm);
        }

        public const uint SHGFI_LARGEICON = 0x000000000;
        public const uint SHGFI_SMALLICON = 0x000000001;
        public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        public const uint SHGFI_ICON = 0x000000100;

        public static readonly Guid FOLDERID_System = new("1AC14E77-02E7-4E5D-B744-2EB1AE5198B7"); // %SystemRoot%\System32
        public static readonly Guid FOLDERID_SystemX86 = new("D65231B0-B2F1-4857-A4CE-A8E7C6EA7D27"); // %SystemRoot%\SysWOW64
        public static readonly Guid FOLDERID_ProgramFiles = new("6D809377-6AF0-444B-8957-A3773F02200E");
        public static readonly Guid FOLDERID_ProgramFilesX86 = new("7C5A40EF-A0FB-4BFC-874A-C0F2E0B9FA8E");
        public static readonly Guid FOLDERID_Windows = new("F38BF404-1D43-42F2-9305-67DE0B28FC23");

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

        [StructLayout(LayoutKind.Sequential)]
        struct SIZE { public int cx, cy; public SIZE(int w, int h) { cx = w; cy = h; } }

        [Flags]
        enum SIIGBF : uint
        {
            SIIGBF_RESIZETOFIT = 0x00,
            SIIGBF_BIGGERSIZEOK = 0x01,
            SIIGBF_MEMORYONLY = 0x02,
            SIIGBF_ICONONLY = 0x04,
            SIIGBF_THUMBNAILONLY = 0x08,
            SIIGBF_INCACHEONLY = 0x10,
        }
        
        enum ASSOCF : uint { NONE = 0 }
        enum ASSOCSTR : uint { EXECUTABLE = 2 }

        public static Image getIconFromFilename(string fName)
        {
            if (Path.HasExtension(fName) == false)
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

        public static string ResolveIndirectString(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return null;

            // Must start with '@' to be a shell indirect string.
            if (!source.TrimStart().StartsWith("@", StringComparison.Ordinal))
                return null;

            var sb = new StringBuilder(1024);
            int hr = SHLoadIndirectString(source, sb, sb.Capacity, IntPtr.Zero);
            return hr == 0 ? sb.ToString() : null; // S_OK
        }

        public static string ExpandKnownFolderPath(string input) // "{GUID}\rest\of\path"
        {
            // Split "{GUID}\something"
            var idx = input.IndexOf('}');
            var guidStr = input.Substring(0, idx + 1);
            var tail = input.Substring(idx + 1).TrimStart('\\');

            var guid = Guid.Parse(guidStr.Trim('{', '}'));
            int hr = SHGetKnownFolderPath(guid, 0, IntPtr.Zero, out var pPath);
            if (hr != 0) Marshal.ThrowExceptionForHR(hr);
            string basePath = Marshal.PtrToStringUni(pPath)!;
            Marshal.FreeCoTaskMem(pPath);

            return string.IsNullOrEmpty(tail) ? basePath : Path.Combine(basePath, tail);
        }

        public static string GetHandlerExecutableForProtocol(string scheme) // e.g., "http", "steam"
        {
            uint cch = 260;
            var sb = new StringBuilder((int)cch);
            uint hr = AssocQueryString(ASSOCF.NONE, ASSOCSTR.EXECUTABLE, scheme, null, sb, ref cch);
            return hr == 0 ? sb.ToString() : null;
        }

        public static Bitmap GetAppsFolderIcon(string id, int size = 32)
        {
            string parsing = $@"shell:AppsFolder\{id}";
            int hr = SHCreateItemFromParsingName(parsing, IntPtr.Zero, typeof(IShellItemImageFactory).GUID, out var factory);
            if (hr != 0 || factory is null) return null;

            factory.GetImage(new SIZE(size, size), SIIGBF.SIIGBF_ICONONLY | SIIGBF.SIIGBF_BIGGERSIZEOK, out var hbm);
            if (hbm == IntPtr.Zero) return null;
            try
            {
                var bmp = Image.FromHbitmap(hbm);
                return bmp;
            }
            finally
            {
                DeleteObject(hbm);
            }
        }
    }
}
