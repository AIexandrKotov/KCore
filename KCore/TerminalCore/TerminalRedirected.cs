using KCore.Graphics.Core;
using KCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.TerminalCore
{
    public class TerminalRedirected
    {
        #region REDIRECTION

        public class DrawingRedirection
        {
            public ComplexPixel[,] Matrix { get; set; }
            public int Left { get; set; }
            public int Top { get; set; }
            public ConsoleColor Fore { get; set; }
            public ConsoleColor Back { get; set; }
            public int Width => Matrix.GetLength(0);
            public int Height => Matrix.GetLength(1);

            public DrawingRedirection()
            {
                Fore = Theme.Fore;
                Back = Theme.Back;
                Left = Terminal.Left;
                Top = Terminal.Top;
                Matrix = new ComplexPixel[Terminal.FixedWindowWidth, Terminal.FixedWindowHeight];
                for (var i = 0; i < Matrix.GetLength(0); i++)
                    for (var j = 0; j < Matrix.GetLength(1); j++)
                        Matrix[i, j] = new ComplexPixel(i, j, ' ', Fore, Back);
            }

            public ConsoleColor ForegroundColor
            {
                get => Fore;
                set => Fore = value;
            }

            public ConsoleColor BackgroundColor
            {
                get => Back;
                set => Back = value;
            }

            public void SetCursorPosition(int left, int top)
            {
                Left = left;
                Top = top;
            }

            public void Write(char[] chars, int index = 0, int count = -1)
            {
                if (count == -1) count = chars.Length - index;
                for (var i = index; i < count; i++)
                {
                    if (Left + 1 > Width) return;
                    Matrix[Left, Top] = Matrix[Left, Top].Update(chars[i], Fore, Back);
                    Left++;
                }
            }

            public void Write(string s)
            {
                for (var i = 0; i < s.Length; i++)
                {
                    if (Left + 1 > Width) return;
                    Matrix[Left, Top] = Matrix[Left, Top].Update(s[i], Fore, Back);
                    Left++;
                }
            }

            public void Clear()
            {
                for (var i = 0; i < Matrix.GetLength(0); i++)
                    for (var j = 0; j < Matrix.GetLength(1); j++)
                        Matrix[i, j] = Matrix[i, j].Update(' ', Fore, Back);
            }

            public void Write(object o) => Write(o.ToString());

            private static IEnumerable<(int, int, T)> Enumerate<T>(T[,] values)
            {
                for (var i = 0; i < values.GetLength(0); i++)
                {
                    for (var j = 0; j < values.GetLength(1); j++)
                    {
                        yield return (i, j, values[i, j]);
                    }
                }
            }

            public Complexive ToComplexive()
            {
                return new Complexive()
                {
                    Width = Width,
                    Height = Height,
                    Pixels = Enumerate(Matrix).Where(x => !(Terminal.FixedWindowWidth - 1 == x.Item1 && Terminal.FixedWindowHeight - 1 == x.Item2)).Select(x => x.Item3).ToArray(),
                };
            }
        }

        internal static DrawingRedirection Redirection { get; set; }
        public static void StartRedirection()
        {
            Redirection = new DrawingRedirection();
        }
        public static DrawingRedirection StopRedirection()
        {
            var red = Redirection;
            Redirection = null;
            return red;
        }
        public static bool Redirected => Redirection != null;

        #endregion
        public static ConsoleColor Fore { get => Redirection.Fore; set => Redirection.Fore = value; }
        public static ConsoleColor Back { get => Redirection.Back; set => Redirection.Back = value; }
        public static int Left { get => Redirection.Left; set => Redirection.Left = value; }
        public static int Top { get => Redirection.Top; set => Redirection.Top = value; }

        public static void Clear()
        {
            Redirection.Clear();
        }
        public static void SetCursorPosition(int left, int top)
        {
            Redirection.SetCursorPosition(left, top);
        }
        public static void Write(string text)
        {
            Redirection.Write(text);
        }
    }
}
