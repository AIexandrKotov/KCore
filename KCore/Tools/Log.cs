using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using KCore.Extensions;

namespace KCore.Tools
{
    public static class Log
    {
        internal const string LogFileName = "kcore.log";

        internal static bool init;

        internal static string prefix = "";

        internal static object locker = new object();

        internal static Queue<string> queue = new Queue<string>(256);

        public static Thread LogThread { get; set; }

        public static bool Logging { get; set; } = false;

        public static void SetPrefix(string s) => prefix = s;

        public static void ClearPrefix() => prefix = "";

        public static string GetString(this DateTime dt)
        {
            var sb = new StringBuilder();
            sb.Append(dt.Day.ToString().PadLeft(2, '0')); sb.Append('.');
            sb.Append(dt.Month.ToString().PadLeft(2, '0')); sb.Append('.');
            sb.Append(dt.Year.ToString()); sb.Append(' ');
            sb.Append(dt.Hour.ToString().PadLeft(2, '0')); sb.Append(':');
            sb.Append(dt.Minute.ToString().PadLeft(2, '0')); sb.Append(':');
            sb.Append(dt.Second.ToString().PadLeft(2, '0')); sb.Append(':');
            sb.Append(dt.Millisecond.ToString().PadLeft(3, '0'));
            return sb.ToString();
        }

        public static void SdkCommand(bool b) => IsInSDK = b;
        static bool IsInSDK = Assembly.GetEntryAssembly() != null && Assembly.GetEntryAssembly().GetName().Name == "BikerSDK";

        public static void Save()
        {
            lock (locker)
            {
#if !DEBUG
                if (!File.Exists(LogFileName)) File.Create(LogFileName).Close();
#endif
                Append(queue.ToArray().JoinIntoString(""));
                queue.Clear();
            }
        }

        public static void Abort()
        {
#if DEBUG
            if (LogThread != null && LogThread.IsAlive) LogThread.Abort();
#endif
        }

        public static void LogProcessor()
        {
            while (true)
            {
                Thread.Sleep(1000);
                lock (locker) if (queue.Count > 0) Save();
            }
        }

        internal static void Init()
        {
            lock (locker)
            {
#if DEBUG
                LogThread = new Thread(LogProcessor);
                LogThread.Name = "Log thread";
                LogThread.Start();
                File.Create(LogFileName).Close();
#else
                if (File.Exists(LogFileName)) File.Delete(LogFileName);
#endif
                init = true;
            }
        }

        internal static void ConWrite(string s)
        {
            if (s.Contains("Exception: ")) System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine(s);
            System.Console.ResetColor();
        }

        internal static void Append(string s)
        {
            if (IsInSDK) ConWrite(s);
            File.AppendAllText(LogFileName, s);
        }

        public static void Add(string s)
        {
            if (!Logging) return;
            if (!init) Init();
            if (s.Contains(Environment.NewLine)) s = Environment.NewLine + s;
            lock (locker) queue.Enqueue($"[{GetString(DateTime.Now)}] {prefix}{s}\n");
        }
    }
}
