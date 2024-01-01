using KCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Editor
{
    internal static class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        public static MainForm MainForm;
        public static ConsoleControl ConsoleControl;
        public static IntPtr ConsoleHandler;
        public static Rect ConsoleRect;

        public static bool NeedRestart = false;
        public static readonly (int, int) PixelSize = (12, 20);
        public static (int, int) Offset = (8, -6);

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var full_path = Assembly.GetEntryAssembly().ManifestModule.FullyQualifiedName.Replace('\\', '_');
            var console = Registry.CurrentUser.OpenSubKey("Console", true);
            var editor = console.OpenSubKey(full_path);
            if (editor == null)
            {
                editor = console.CreateSubKey(full_path);
                NeedRestart = true;
                editor.SetValue("QuickEdit", 0, RegistryValueKind.DWord);
                editor.SetValue("LineSelection", 0, RegistryValueKind.DWord);
                editor.SetValue("CursorType", 0, RegistryValueKind.DWord);
                editor.SetValue("FaceName", "Lucida Console", RegistryValueKind.String);
                editor.SetValue("FontFamily", 54, RegistryValueKind.DWord);
                editor.SetValue("FontSize", 1310720, RegistryValueKind.DWord);
                editor.SetValue("FontWeight", 400, RegistryValueKind.DWord);
                return;
            }

            var KCore_Thread = new Thread(() =>
            {
                Console.Title = "KCore Editor Console";
                while (ConsoleHandler == IntPtr.Zero)
                    ConsoleHandler = FindWindow(null, Console.Title);
                Terminal.Init(80, 25);
                Terminal.UpdatesPerSecond = 0;
                Terminal.KeySplit = 1;
                Terminal.KeyWait = 1;
                (ConsoleControl = new ConsoleControl()).Start();
            });
            KCore_Thread.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(MainForm = new MainForm());
        }
    }
}
