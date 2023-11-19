using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KCore.TerminalCore
{
    internal class TerminalBase
    {
        #region WINAPI
        [DllImport("Kernel32.dll")]
        internal static extern bool GetCurrentConsoleFont(IntPtr hConsoleOutput, bool bMaximumWindow, out CONSOLE_FONT_INFO lpConsoleCurrentFont);
        [DllImport("Kernel32.dll")]
        internal static extern bool SetConsoleFont(IntPtr hConsoleOutput, ushort lpConsolesbSize);
        [DllImport("User32.dll")]
        internal static extern byte GetKeyState(byte nVirtKey);
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("User32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("Kernel32.dll")]
        internal static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput, out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

        [DllImport("kernel32.dll")]
        internal static extern COORD GetLargestConsoleWindowSize(IntPtr hConsoleOutput);

        internal static IntPtr CachedOutputHandle;
        //private const uint STD_INPUT_HANDLE = uint.MaxValue - 9;
        private const int STD_OUTPUT_HANDLE = -11;
        //private const uint STD_ERROR_HANDLE = uint.MaxValue - 11;

        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        internal struct COORD
        {
            public short X;
            public short Y;

            public static implicit operator ValueTuple<int, int>(COORD c)
            {
                return (c.X, c.Y);
            }
        }

        internal struct CONSOLE_FONT_INFO
        {
            public uint nFont;
            public COORD dwFontSize;
        }

        internal struct SMALL_RECT
        {
            public short Left, Top, Right, Bottom;
        }

        internal struct CONSOLE_SCREEN_BUFFER_INFO
        {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public ushort wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;

            public (int, int) WindowSize() => (WindowWidth(), WindowHeight());
            public (int, int) BufferSize() => (BufferWidth(), BufferHeight());

            public int WindowWidth() => srWindow.Right - srWindow.Left + 1;
            public int WindowHeight() => srWindow.Bottom - srWindow.Top + 1;
            public int BufferWidth() => dwSize.X;
            public int BufferHeight() => dwSize.Y;
        }
        internal static CONSOLE_SCREEN_BUFFER_INFO GetBufferInfo()
        {
            GetConsoleScreenBufferInfo(CachedOutputHandle, out var csbi);
            return csbi;
        }
        internal static COORD GetLargestWindowSize() => GetLargestConsoleWindowSize(CachedOutputHandle);
        #endregion

        static TerminalBase()
        {
            Console.CursorVisible = false;
            CachedOutputHandle = GetStdHandle(STD_OUTPUT_HANDLE);
        }
    }
}
