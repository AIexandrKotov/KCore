using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace conpaint
{
    public static class ConpaintConsole
    {
        public static readonly object Sync = new object();
        public static readonly ConcurrentQueue<Task> Actions = new ConcurrentQueue<Task>();

        public static void ConpaintConsoleMain()
        {
            while (true)
            {
                Monitor.Enter(Sync);
                try
                {
                    Monitor.Wait(Sync);
                    while (Actions.Count > 0)
                        if (Actions.TryDequeue(out var task))
                            task.RunSynchronously();
                }
                finally
                {
                    Monitor.Exit(Sync);
                }
            }
        }

        public static bool ConpaintPrepare() // returns need restart
        {
            var full_path = Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName.Replace('\\', '_').Replace("conpaint.dll", "conpaint.exe");
            var console = Registry.CurrentUser.OpenSubKey("Console", true);
            var editor = console.OpenSubKey(full_path);
            if (editor == null)
            {
                editor = console.CreateSubKey(full_path);
                editor.SetValue("QuickEdit", 0, RegistryValueKind.DWord);
                editor.SetValue("LineSelection", 0, RegistryValueKind.DWord);
                editor.SetValue("CursorType", 0, RegistryValueKind.DWord);
                editor.SetValue("FaceName", "Lucida Console", RegistryValueKind.String);
                editor.SetValue("FontFamily", 54, RegistryValueKind.DWord);
                editor.SetValue("FontSize", 1310720, RegistryValueKind.DWord);
                editor.SetValue("FontWeight", 400, RegistryValueKind.DWord);
                return true;
            }
            return false;
        }

        public static void Start()
        {
            Console.Title = "conpaint";
            if (ConpaintPrepare())
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new RestartingForm());
            }

            new Thread(ConpaintConsoleMain)
            {
                Name = "ConpaintConsole",
                IsBackground = true,
            }.Start();
        }
    }
}
