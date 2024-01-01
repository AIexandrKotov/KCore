using KCore.Storage;
using KCore.CoreForms;
using KCore.Graphics;
using KCore.Graphics.Core;
using KCore.TerminalCore;
using KCore.Theming;
using KCore.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace KCore
{
    /// <summary>
    /// Методы для прямого взаимодействия с консолью
    /// </summary>
    public static class Terminal
    {
        #region Key extensions
        public static bool Pressed(this byte b) =>  TerminalBase.GetKeyState(b) >> 7 == Key.ActiveKey;
        public static bool Triggered(this byte b) => TerminalBase.GetKeyState(b) << 7 == Key.TriggerKey;
        #endregion

        #region Key control
        internal static event Action<byte> OnKeyUp;
        internal static event Action<byte> OnKeyDown;

        internal static void AddKeyDown(Action<byte> pressKey) => OnKeyDown += pressKey;
        internal static void ClearKeyDown(Action<byte> pressKey) => OnKeyDown -= pressKey;
        internal static void AddKeyUp(Action<byte> pressKey) => OnKeyUp += pressKey;
        internal static void ClearKeyUp(Action<byte> pressKey) => OnKeyUp -= pressKey;

        private static int cached_ups = 70;
        private static void ManualTools(byte x)
        {
        }
        private static void UpKeyMethod(byte x) { }

        internal static Thread KeyThread { get; set; }

        private static int keywait = 20, keysplit = 3;
        private static bool onlyOneRepeation = true;

        public static bool OnlyOneRepeation { get => onlyOneRepeation; set => onlyOneRepeation = value; }

        public static int KeyWait
        {
            get => keywait;
            set
            {
                keywait = value;
            }
        }
        public static int KeySplit
        {
            get => keysplit;
            set
            {
                keysplit = value;
            }
        }

        //public static bool KeyRestrictedPerfomance { get => restrictedPerfomance; set => restrictedPerfomance = value; }

        private static bool KeyThreadLoop = true;
        private static bool restrictedPerfomance = true;
        private static void KeyProcessor()
        {
            var pressed_keys = new int[256];
            var repeation = -1;
            byte current;
            while (KeyThreadLoop)
            {
                Thread.Sleep(1);
                if (!WindowIsActive)
                {
                    Thread.Sleep(500);
                    continue;
                }
                if (!onlyOneRepeation) repeation = -1;
                for (var i = 0; i < 255; i++)
                {
                    current = (byte)i;
                    if (current.Pressed())
                    {
                        if (pressed_keys[i] == 0)
                        {
                            OnKeyDown(current);
                        }
                        else if (pressed_keys[i] > keywait && pressed_keys[i] % keysplit == 0 && (repeation == -1 || repeation == i))
                        {
                            OnKeyDown(current);
                            repeation = i;
                        }

                        pressed_keys[i] += 1;
                    }
                    else
                    {
                        if (pressed_keys[i] == 0) continue;
                        else
                        {
                            OnKeyUp(current);
                            if (repeation == i) repeation = -1;
                            pressed_keys[i] = 0;
                        }
                    }
                }
            }
        }

        internal static void KeyThreadStart()
        {
            KeyThread = new Thread(KeyProcessor);
            KeyThread.Name = "Input Key";
            KeyThread.Start();
        }

        public static void Abort()
        {
            KeyThreadLoop = false;
            Thread.Sleep(500);
            KeyThread.Abort();
            Tools.Log.Abort();
            OnKeyDown -= ManualTools;
            OnKeyUp -= UpKeyMethod;
        }
        #endregion

        #region Base API
        public static bool CursorVisible { get => Console.CursorVisible; set => Console.CursorVisible = value; }
        public static ConsoleColor Back
        {
            get
            {
                if (TerminalRedirected.Redirected) return TerminalRedirected.Back;
                else return Console.BackgroundColor;
            }
            set
            {
                if (TerminalRedirected.Redirected) TerminalRedirected.Back = value;
                else Console.BackgroundColor = value;
            }
        }
        public static ConsoleColor Fore
        {
            get
            {
                if (TerminalRedirected.Redirected) return TerminalRedirected.Fore;
                else return Console.ForegroundColor;
            }
            set
            {
                if (TerminalRedirected.Redirected) TerminalRedirected.Fore = value;
                else Console.ForegroundColor = value;
            }
        }

        public static int Left
        {
            get => TerminalRedirected.Redirected ? TerminalRedirected.Left : Console.CursorLeft; set
            {
                if (TerminalRedirected.Redirected) TerminalRedirected.Left = value;
                else Console.CursorLeft = value;
            }
        }
        public static int Top
        {
            get => TerminalRedirected.Redirected ? TerminalRedirected.Top : Console.CursorTop; set
            {
                if (TerminalRedirected.Redirected) TerminalRedirected.Top = value;
                else Console.CursorTop = value;
            }
        }
        public static void Clear()
        {
            if (TerminalRedirected.Redirected) TerminalRedirected.Clear();
            else Console.Clear();
        }
        public static void Set(int left, int top)
        {
            if (TerminalRedirected.Redirected) TerminalRedirected.SetCursorPosition(left, top);
            else Console.SetCursorPosition(left, top);
        }
        public static void Write(object o)
        {
            if (TerminalRedirected.Redirected) TerminalRedirected.Write(o.ToString());
            else Console.Write(o.ToString());
        }
        public static void Write(string text)
        {
            if (TerminalRedirected.Redirected) TerminalRedirected.Write(text);
            else Console.Write(text);
        }
        #endregion

        #region Extended KCore API

        public const int DefaultUpdatesPerSecond = 70;

        private static int minimalWindowWidth, minimalWindowHeight;

        private static int fww;
        private static int fwh;
        private static int ups = DefaultUpdatesPerSecond;
        private static bool inited;
        private static bool elderThanWS2016;
        private static readonly object syncresize = new object();
        private const string WINDOW_SIZE_TOO_SMALL_EXCEPTION_STRING = "Window size too small";
        internal static TimeSpan UPS { get; private set; } = new TimeSpan((long)(TimeSpan.TicksPerSecond / (double)DefaultUpdatesPerSecond));
        private static IntPtr ConsoleWindow;
        internal static bool WindowIsActive { get => ConsoleWindow == TerminalBase.GetForegroundWindow(); }


        private class SetByAlignment : BoundedObject
        {
            public static SetByAlignment This = new SetByAlignment();

            public SetByAlignment()
            {
                Container = TerminalContainer.This;
            }
            public override (int, int) Draw(int left, int top)
            {
                throw new NotImplementedException();
            }

            public override (int, int) Clear(int left, int top)
            {
                throw new NotImplementedException();
            }
        }
        public static void Set(Alignment alignment, int left, int top)
        {
            SetByAlignment.This.Left = left;
            SetByAlignment.This.Top = top;
            SetByAlignment.This.Alignment = alignment;
            (left, top) = SetByAlignment.This.GetCorner();
            Set(left, top);
        }

        #endregion



        #region Resize
        internal static void SetWindowSize() => Console.SetWindowSize(FixedWindowWidth, FixedWindowHeight);

        internal static void Clear(ConsoleColor backgroundcolor)
        {
            Back = backgroundcolor;
            Clear();
        }
        internal static void Resize() => Resize(Themes.Current);
        internal static void Resize(ConsoleColor backgroundcolor) => Resize(Themes.Current, backgroundcolor);
        internal static void Resize(ColorTheme colorSet) => Resize(colorSet, colorSet.Back);
        internal static void InternalResize(ColorTheme colorSet, ConsoleColor backgroundColor)
        {
            lock (syncresize)
            {
                var largest = TerminalBase.GetLargestWindowSize();

                if (largest.X < minimalWindowWidth || largest.Y < minimalWindowHeight)
                {
                    Clear(backgroundColor);
                }

                if (FixedWindowWidth > largest.X || FixedWindowHeight > largest.Y)
                {
                    if (FixedWindowWidth > largest.X) FixedWindowWidth = largest.X;
                    if (FixedWindowHeight > largest.Y) FixedWindowHeight = largest.Y;
                    SetWindowSize();
                }

                var bi = TerminalBase.GetBufferInfo();

                if (WindowSizeExternalManage)
                {
                    if (bi.WindowWidth() < minimalWindowWidth || bi.WindowHeight() < minimalWindowHeight)
                    {
                        if (bi.WindowWidth() < minimalWindowWidth) FixedWindowWidth = minimalWindowWidth;
                        if (bi.WindowHeight() < minimalWindowHeight) FixedWindowHeight = minimalWindowHeight;
                    }
                    else
                    {
                        FixedWindowWidth = bi.WindowWidth();
                        FixedWindowHeight = bi.WindowHeight();
                    }
                }

                Clear(backgroundColor);

                if (bi.WindowWidth() > FixedWindowWidth || bi.WindowHeight() > FixedWindowHeight) Console.SetWindowSize(FixedWindowWidth, FixedWindowHeight);
                Console.SetBufferSize(FixedWindowWidth, FixedWindowHeight);
                SetWindowSize();
            }
        }
        internal static void Resize(ColorTheme colorSet, ConsoleColor backgroundColor)
        {
            if (elderThanWS2016) InternalResize(colorSet, backgroundColor);
            else
            {
                var fuck_v2_console = true;
                while (fuck_v2_console)
                {
                    fuck_v2_console = false;
                    try
                    {
                        InternalResize(colorSet, backgroundColor);
                    }
                    catch (Exception)
                    {
                        fuck_v2_console = true;
                    }
                    CursorVisible = false;
                }
            }
        }

        #endregion
        /// <summary>
        /// Возвращает true, если консоль запущена с помощью новой оболочки
        /// </summary>
        public static bool ForceV2 { get; private set; }
        internal static bool ElderThanWS2016 => elderThanWS2016;

        public static int MinimalWindowWidth { get => minimalWindowWidth; }
        public static int MinimalWindowHeight { get => minimalWindowHeight; }

        /// <summary>
        /// Возвращает или задаёт фиксированную ширину окна консоли
        /// </summary>
        public static int FixedWindowWidth
        {
            get => fww;
            set
            {
                lock (syncresize) fww = value;
            }
        }

        /// <summary>
        /// Возвращает или задаёт фиксированную высоту окна консоли
        /// </summary>
        public static int FixedWindowHeight
        {
            get => fwh;
            set
            {
                lock (syncresize) fwh = value;
            }
        }

        /// <summary>
        /// Возвращает или задаёт количество обновлений консоли в секунду
        /// </summary>
        public static int UpdatesPerSecond
        {
            get => ups;
            set
            {
                ups = value;
                if (value != 0) UPS = new TimeSpan((long)(TimeSpan.TicksPerSecond / (double)value));
                else UPS = TimeSpan.Zero;
            }
        }

        public static bool UpdateWindowInactive { get; set; } = false;

        /// <summary>
        /// Возвращает true, если размер консоли разрешено менять курсором
        /// </summary>
        public static bool WindowSizeExternalManage
        {
            get;
            set;
        } = true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// Устанавливает для цветов фона и текста консоли их значения по умолчанию исходя из текущей темы
        /// </summary>
        public static void ResetColor()
        {
            Back = Theme.Back;
            Fore = Theme.Fore;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// Устанавливает для цвета текста консоли значение по умолчанию исходя из текущей темы
        /// </summary>
        public static void ResetFore()
        {
            Fore = Theme.Fore;
        }

        public static void Init() => Init(80, 25);

        public static void Init(int width, int height)
        {
            ConsoleWindow = TerminalBase.FindWindow(null, Console.Title);
            if (width != minimalWindowWidth || height != minimalWindowHeight)
            {
                minimalWindowWidth = width;
                minimalWindowHeight = height;
                fww = minimalWindowWidth;
                fwh = minimalWindowHeight;
                ResetColor();
                Resize();
            }
            CursorVisible = false;

            if (!inited)
            {
                Console.CancelKeyPress += (o, e) => e.Cancel = true;
                var win = ((string)Microsoft.Win32.Registry
                    .GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", "Windows 7"))
                    .Split(new char[] { ' ', '\r', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                elderThanWS2016 = win[1] == "7" || win[1] == "Vista" || win[1] == "XP";
                if (!elderThanWS2016)
                {
                    var fv2 = (int)Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Console", "ForceV2", 0);
                    ForceV2 = fv2 == 1;
                    WindowSizeExternalManage = true;
                }
                else
                {
                    WindowSizeExternalManage = false;
                    ForceV2 = true;
                }
                KeyThreadStart();

                ResetColor();
                Resize();
                inited = true;
                OnKeyDown += ManualTools;
                OnKeyUp += UpKeyMethod;
                Log.Logging = true;
            }
        }

        static Terminal()
        {
            if (!inited) Init();
        }
    }
}
