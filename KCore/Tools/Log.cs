using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using KCore.Extensions.InsteadSLThree;

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
            return dt.ToString("dd.MM.yyyy HH:mm:ss:fff");
        }

        public static void Save()
        {
            lock (locker)
            {
                if (!File.Exists(LogFileName)) File.Create(LogFileName).Close();
                Append(queue.ToArray().JoinIntoString(""));
                queue.Clear();
            }
        }

        public static void Abort()
        {
            if (LogThread != null && LogThread.IsAlive) LogThread.Abort();
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
                LogThread = new Thread(LogProcessor);
                LogThread.Name = "Log thread";
                LogThread.Start();
                if (File.Exists(LogFileName)) File.Delete(LogFileName);
                File.Create(LogFileName).Close();
                init = true;
            }
        }

        internal static void Append(string s)
        {
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
