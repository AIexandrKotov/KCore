using KCore.Extensions;
using KCore.Graphics.Core;
using KCore.Graphics;
using System;
using System.Collections.Generic;
using static KCore.Terminal;

namespace KCore.Graphics.Uncontrolable
{
    public static class Graph
    {
        public static void DrawAll(params BoundedObject[] boundedObjects)
        {
            for (var i = 0; i < boundedObjects.Length; i++) boundedObjects[i].Draw();
        }

        #region Base Graph Functions
        private const string percentstr = "%";
        private static string[] cachedSpaces = new string[1000];

        static Graph()
        {
            cachedSpaces[0] = "";
            for (var i = 1; i < cachedSpaces.Length; i++)
                cachedSpaces[i] = new string(' ', i);
        }

        internal static TextAlignment GetTextAlignment(Alignment alignment)
        {
            if ((alignment & Alignment.RightWidth) == Alignment.RightWidth) return TextAlignment.Right;
            if ((alignment & Alignment.CenterWidth) == Alignment.CenterWidth) return TextAlignment.Center;
            if ((alignment & Alignment.LeftWidth) == Alignment.LeftWidth) return TextAlignment.Left;
            throw new ArgumentException();
        }

        public static void ResetColor() => Console.ResetColor();

        public static int ConsoleWidth => FixedWindowWidth;
        public static int ConsoleHeight => FixedWindowHeight;

        public static ValueTuple<int, int> CursorPosition
        {
            get => TerminalCore.TerminalBase.GetBufferInfo().dwCursorPosition;
            set => Set(value.Item1, value.Item2);
        }

        public static void SafeLine(int left, int top, int right, int bottom, ConsoleColor color)
        {
            Func<double, double> function = x => (bottom - top) / (double)(right - left) * (x - left) + top;
            Func<double, double> inverse = y => (right - left) / ((double)bottom - top) * (y - top) + left;
            var pixels = new List<(int, int)>(Math.Abs(right - left) + Math.Abs(bottom - top));
            if (right >= left)
            {
                for (var x = left; x <= right; x++)
                {
                    var y = function(x);
                    if (double.IsInfinity(y) || double.IsNaN(y)) continue;
                    var t = (x, y.Trunc());
                    if (t.Item2 < 0 || t.Item2 >= FixedWindowHeight - 1) continue;
                    if (!pixels.Contains(t)) pixels.Add(t);
                }
            }
            else
            {
                for (var x = left; x >= right; x--)
                {
                    var y = function(x);
                    if (double.IsInfinity(y) || double.IsNaN(y)) continue;
                    var t = (x, y.Trunc());
                    if (t.Item2 < 0 || t.Item2 >= FixedWindowHeight - 1) continue;
                    if (!pixels.Contains(t)) pixels.Add(t);
                }
            }

            if (bottom >= top)
            {
                for (var y = top; y <= bottom; y++)
                {
                    var x = inverse(y);
                    if (double.IsInfinity(x) || double.IsNaN(x)) continue;
                    var t = (x.Trunc(), y);
                    if (t.Item1 < 0 || t.Item1 >= FixedWindowWidth - 1) continue;
                    if (!pixels.Contains(t)) pixels.Add(t);
                }
            }
            else
            {
                for (var y = top; y >= bottom; y--)
                {
                    var x = inverse(y);
                    if (double.IsInfinity(x) || double.IsNaN(x)) continue;
                    var t = (x.Trunc(), y);
                    if (t.Item1 < 0 || t.Item1 >= FixedWindowWidth - 1) continue;
                    if (!pixels.Contains(t)) pixels.Add(t);
                }
            }
            pixels.RemoveAll(x => x.Item1 < 0 || x.Item2 < 0 || x.Item1 >= FixedWindowWidth || x.Item2 >= FixedWindowHeight);
            Back = color;
            for (var i = 0; i < pixels.Count; i++)
            {
                Set(pixels[i].Item1, pixels[i].Item2);
                Write(' ');
            }
            ResetColor();
        }

        public static void Draw(List<(int, int)> pixels, char ch)
        {
            pixels.RemoveAll(x => x.Item1 < 0 || x.Item2 < 0 || x.Item1 >= FixedWindowWidth || x.Item2 >= FixedWindowHeight);
            for (var i = 0; i < pixels.Count; i++)
            {
                Set(pixels[i].Item1, pixels[i].Item2);
                Write(ch);
            }
        }

        public static void SafeLine(List<(int, int)> pixels, int left, int top, int right, int bottom)
        {
            double function(double x) => (bottom - top) / (double)(right - left) * (x - left) + top;
            double inverse(double y) => (right - left) / ((double)bottom - top) * (y - top) + left;

            if (right >= left)
            {
                for (var x = left; x <= right; x++)
                {
                    var y = function(x);
                    if (double.IsInfinity(y) || double.IsNaN(y)) continue;
                    var t = (x, y.Trunc());
                    if (t.Item2 < 0 || t.Item2 >= FixedWindowHeight - 1) continue;
                    if (!pixels.Contains(t)) pixels.Add(t);
                }
            }
            else
            {
                for (var x = left; x >= right; x--)
                {
                    var y = function(x);
                    if (double.IsInfinity(y) || double.IsNaN(y)) continue;
                    var t = (x, y.Trunc());
                    if (t.Item2 < 0 || t.Item2 >= FixedWindowHeight - 1) continue;
                    if (!pixels.Contains(t)) pixels.Add(t);
                }
            }

            if (bottom >= top)
            {
                for (var y = top; y <= bottom; y++)
                {
                    var x = inverse(y);
                    if (double.IsInfinity(x) || double.IsNaN(x)) continue;
                    var t = (x.Trunc(), y);
                    if (t.Item1 < 0 || t.Item1 >= FixedWindowWidth - 1) continue;
                    if (!pixels.Contains(t)) pixels.Add(t);
                }
            }
            else
            {
                for (var y = top; y >= bottom; y--)
                {
                    var x = inverse(y);
                    if (double.IsInfinity(x) || double.IsNaN(x)) continue;
                    var t = (x.Trunc(), y);
                    if (t.Item1 < 0 || t.Item1 >= FixedWindowWidth - 1) continue;
                    if (!pixels.Contains(t)) pixels.Add(t);
                }
            }
        }

        public static void SafeLine(int left, int top, int right, int bottom, char ch)
        {
            double function(double x) => (bottom - top) / (double)(right - left) * (x - left) + top;
            double inverse(double y) => (right - left) / ((double)bottom - top) * (y - top) + left;

            var pixels = new List<(int, int)>(Math.Abs(right - left) + Math.Abs(bottom - top));
            if (right >= left)
            {
                for (var x = left; x <= right; x++)
                {
                    var y = function(x);
                    if (double.IsInfinity(y) || double.IsNaN(y)) continue;
                    var t = (x, y.Trunc());
                    if (t.Item2 < 0 || t.Item2 >= FixedWindowHeight - 1) continue;
                    if (!pixels.Contains(t)) pixels.Add(t);
                }
            }
            else
            {
                for (var x = left; x >= right; x--)
                {
                    var y = function(x);
                    if (double.IsInfinity(y) || double.IsNaN(y)) continue;
                    var t = (x, y.Trunc());
                    if (t.Item2 < 0 || t.Item2 >= FixedWindowHeight - 1) continue;
                    if (!pixels.Contains(t)) pixels.Add(t);
                }
            }

            if (bottom >= top)
            {
                for (var y = top; y <= bottom; y++)
                {
                    var x = inverse(y);
                    if (double.IsInfinity(x) || double.IsNaN(x)) continue;
                    var t = (x.Trunc(), y);
                    if (t.Item1 < 0 || t.Item1 >= FixedWindowWidth - 1) continue;
                    if (!pixels.Contains(t)) pixels.Add(t);
                }
            }
            else
            {
                for (var y = top; y >= bottom; y--)
                {
                    var x = inverse(y);
                    if (double.IsInfinity(x) || double.IsNaN(x)) continue;
                    var t = (x.Trunc(), y);
                    if (t.Item1 < 0 || t.Item1 >= FixedWindowWidth - 1) continue;
                    if (!pixels.Contains(t)) pixels.Add(t);
                }
            }
            pixels.RemoveAll(x => x.Item1 < 0 || x.Item2 < 0 || x.Item1 >= FixedWindowWidth || x.Item2 >= FixedWindowHeight);
            for (var i = 0; i < pixels.Count; i++)
            {
                Set(pixels[i].Item1, pixels[i].Item2);
                Write(ch);
            }
        }

        public static void Tabulate(int leftx, int rightx, Func<double, double> function)
        {
            Back = Theme.Border;
            for (var x = leftx; x < rightx; x++)
            {
                var y = function(x).Trunc();
                Set(x, y);
                Write(' ');
            }
            ResetColor();
        }

        public static void OutSpaces(int count)
        {
            Write(Space(count));
        }

        public static void OutWhile(int widthwhile)
        {
            var left = Left;
            if (widthwhile - left <= 0) return;
            Write(Space(widthwhile - left));
        }

        public static void OutChars(char c, int count)
        {
            Write(Chars(c, count));
        }

        public static string Align(this string text, TextAlignment alignment, int width)
        {
            switch (alignment)
            {
                case TextAlignment.Left: return text.PadRight(width);
                case TextAlignment.Center: return text.PadCenter(width);
                case TextAlignment.Right: return text.PadLeft(width);
            }
            throw new ArgumentException();
        }

        public static void AlignCursorPosition(string text, TextAlignment textAlignment, int left, int top, int width)
        {
            switch (textAlignment)
            {
                case TextAlignment.Left:
                    Set(left - text.Length, top);
                    return;
                case TextAlignment.Center:
                    Set(left + width / 2 - text.Length / 2, top);
                    return;
                case TextAlignment.Right:
                    Set(left + width - text.Length, top);
                    return;
            }
        }

        public static void OutPercentString(double d)
        {
            Write(GetPercentString(d));
        }
        public static void OutPercentString(int left, int top, double d)
        {
            Set(left, top);
            Write(GetPercentString(d));
        }

        public static void OutPercentString(float d)
        {
            Write(GetPercentString(d));
        }
        public static void OutPercentString(int left, int top, float d)
        {
            Set(left, top);
            Write(GetPercentString(d));
        }

        public static void OutPercentString(double d, int digits)
        {
            Write(GetPercentString(d, digits));
        }
        public static void OutPercentString(int left, int top, double d, int digits)
        {
            Set(left, top);
            Write(GetPercentString(d, digits));
        }

        public static void OutPercentString(float d, int digits)
        {
            Write(GetPercentString(d, digits));
        }
        public static void OutPercentString(int left, int top, float d, int digits)
        {
            Set(left, top);
            Write(GetPercentString(d, digits));
        }

        public static string Space(int count) => count < cachedSpaces.Length ? cachedSpaces[count] : new string(' ', count);
        public static string Chars(char c, int count) => new string(c, count);
        public static string GetPercentString(double d) => (d * 100).Round() + percentstr;
        public static string GetPercentString(float f) => (f * 100).Round() + percentstr;
        public static string GetPercentString(double d, int digits) => (d * 100).Round(digits) + percentstr;
        public static string GetPercentString(float f, int digits) => (f * 100).Round(digits) + percentstr;

        public static string GetInfluenceString(double d, int digits = 2)
        {
            var k = ((d - 1) * 100).Round(digits);
            return (k < 0 ? "-" : k > 0 ? "+" : "") + Math.Abs(k) + percentstr;
        }

        public static string GetInfluenceString(float f, int digits = 2)
        {
            var k = ((f - 1) * 100).Round(digits);
            return (k < 0 ? "-" : k > 0 ? "+" : "") + Math.Abs(k) + percentstr;
        }

        public static string GetInfluenceDecimalString(double d, int digits = 1)
        {
            d = d.Round(digits);
            return (d < 0 ? "-" : d > 0 ? "+" : "") + Math.Abs(d);
        }

        public static string GetInfluenceDecimalString(float f, int digits = 1)
        {
            f = f.Round(digits);
            return (f < 0 ? "-" : f > 0 ? "+" : "") + Math.Abs(f);
        }

        public static string GetTextTimeString(double seconds, string days, string hours, string mins, string secs)
        {
            var m = seconds / 60;
            var h = m / 60;
            var d = h / 24;
            if (m < 10) return $"{seconds.Round()} {secs}";
            else if (h < 10) return $"{m.Round()} {mins}";
            else if (d < 4) return $"{h.Round()} {hours}";
            else return $"{d.Round()} {days}";
        }

        private static (int, string)[] getdecimaloptimalprefixes = new (int, string)[]
        {
            (1000, "k"),
            (1000000, "kk"),
            (1000000000, "kkk"),
        };

        public static string GetDecimalOptimalString(double d)
        {
            for (var i = getdecimaloptimalprefixes.Length - 1; i >= 0; i--)
            {
                if (d / getdecimaloptimalprefixes[i].Item1 >= 1) return $"{(d / getdecimaloptimalprefixes[i].Item1).Round(1)}{getdecimaloptimalprefixes[i].Item2}";
            }
            return d.Round(1).ToString();
        }

        internal static void Draw(Line line, ConsoleColor color)
        {
            Back = color;
            Set(line.PositionLeft, line.PositionTop);
            Write(Space(line.Length));
            ResetColor();
        }

        internal static void Draw(int left, int top, Line line, ConsoleColor color)
        {
            Back = color;
            Set(left + line.PositionLeft, top + line.PositionTop);
            Write(Space(line.Length));
            ResetColor();
        }

        internal static void Draw(Primitive prim, ConsoleColor color)
        {
            Back = color;
            for (var i = 0; i < prim.Lines.Length; i++)
            {
                Set(prim.Lines[i].PositionLeft, prim.Lines[i].PositionTop);
                Write(Space(prim.Lines[i].Length));
            }
            ResetColor();
        }

        internal static void Draw(int left, int top, Primitive prim, ConsoleColor color)
        {
            Back = color;
            for (var i = 0; i < prim.Lines.Length; i++)
            {
                Set(left + prim.Lines[i].PositionLeft, top + prim.Lines[i].PositionTop);
                Write(Space(prim.Lines[i].Length));
            }
            ResetColor();
        }

        public static void FillRows(int startline, int length, ConsoleColor color)
        {
            Back = color;
            for (var i = 0; i < length; i++)
            {
                Set(0, startline + i);
                Write(Space(FixedWindowWidth));
            }
            ResetColor();
        }
        internal static void FillRows(int startline, int length) => FillRows(startline, length, Theme.Fore);
        internal static void FillRows(int length, ConsoleColor color) => FillRows(0, length, color);
        internal static void FillRows(int length) => FillRows(length, Theme.Fore);
        public static void WriteColor(ConsoleColor color, string str)
        {
            Fore = color;
            Write(str);
        }
        internal static void WriteColor(ConsoleColor color, params string[] str)
        {
            Fore = color;
            Write(str.JoinIntoString(""));
        }
        public static void Row(ConsoleColor color, int startleft, int starttop, int length)
        {
            Set(startleft, starttop);
            Back = color;
            Write(Space(length));
            ResetColor();
        }
        public static void Row(int startleft, int starttop, int length)
        {
            Set(startleft, starttop);
            Write(Space(length));
        }
        public static void Row(int startleft, int starttop, int length, char ch)
        {
            Set(startleft, starttop);
            Write(Chars(ch, length));
        }
        public static void Column(ConsoleColor color, int startleft, int starttop, int length)
        {
            Back = color;
            for (var i = starttop; i < starttop + length; i++)
            {
                Set(startleft, i);
                Write(' ');
            }
            ResetColor();
        }
        public static void Column(int startleft, int starttop, int length)
        {
            for (var i = starttop; i < starttop + length; i++)
            {
                Set(startleft, i);
                Write(' ');
            }
        }
        public static void Column(int startleft, int starttop, int length, char ch)
        {
            for (var i = starttop; i < starttop + length; i++)
            {
                Set(startleft, i);
                Write(ch);
            }
        }
        #endregion

        internal static void FillRectangle(ConsoleColor color, int startleft, int starttop, int width, int height)
        {
            Back = color;
            for (var i = starttop; i < starttop + height; i++)
                Row(color, startleft, i, width);
        }

        public static void ChooseEffect(ConsoleColor color, int startleft, int starttop, int width, int height)
        {
            Back = color;
            Set(startleft, starttop);
            Write(Space(2));
            Set(startleft + width - 2, starttop);
            Write(Space(2));
            Set(startleft, starttop + 1);
            Write(Space(1));
            Set(startleft + width - 1, starttop + 1);
            Write(Space(1));

            Set(startleft, starttop + height - 2);
            Write(Space(1));
            Set(startleft + width - 1, starttop + height - 2);
            Write(Space(1));
            Set(startleft, starttop + height - 1);
            Write(Space(2));
            Set(startleft + width - 2, starttop + height - 1);
            Write(Space(2));
            ResetColor();
        }

        //public static void ChooseEffect(Rectangle rectangle) => ChooseEffect(ConsoleColor.Black, rectangle);
        /*public static void ChooseEffect(ConsoleColor color, Rectangle rectangle)
        {
            ChooseEffect(color, rectangle.GetLeftCornerValue(), rectangle.GetTopCornerValue(), rectangle.Width, rectangle.Height);
        }*/

        public static void ChooseEffect(int startleft, int starttop, int width, int height) => ChooseEffect(ConsoleColor.Black, startleft, starttop, width, height);

        public static void Bar(int startleft, int starttop, double percent, ConsoleColor color1, ConsoleColor color2, int width)
        {
            var left = (percent * width).Round();
            var right = width - left;
            Set(startleft, starttop);
            Back = color1;
            Write(Space(left));
            Back = color2;
            Write(Space(right));
            ResetColor();
        }

        public static void StripedBar(int startleft, int starttop, double percent, ConsoleColor color1, ConsoleColor color2, ConsoleColor foreground, int width)
        {
            var left = (percent * width).Round();
            var right = width - left;
            Set(startleft, starttop);
            Fore = foreground;
            Back = color1;
            Write(Chars('▌', left));
            Back = color2;
            Write(Chars('▌', right));
            ResetColor();
        }

        public static void StripedBar(int startleft, int starttop, double percent, ConsoleColor color1, ConsoleColor color2, int width) =>
            StripedBar(startleft, starttop, percent, color1, color2, Theme.Back, width);

        public static void DoubleBar(int startleft, int starttop, double percent, ConsoleColor color1, ConsoleColor color2, int width)
        {
            var _left = (percent * width).Round();
            var left = Space(_left);
            var right = Space(width - _left);
            Set(startleft, starttop);
            Back = color1;
            Write(left);
            Back = color2;
            Write(right);
            Set(startleft, starttop + 1);
            Back = color1;
            Write(left);
            Back = color2;
            Write(right);
            ResetColor();
        }
    }
}
