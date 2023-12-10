using KCore.Extensions;
using KCore.Extensions.InsteadSLThree;
using KCore.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCore.Graphics.Core
{
    public class Complexive : ICloneable
    {
        public static class Convertion
        {
            public struct BaseComplexPixel
            {
                public int R, G, B;
                public ConsoleColor Foreground, Background;
                public char Character;

                public ComplexPixel ToConcretePixel(int left, int top) => new ComplexPixel(left, top, Character, Foreground, Background);
            }

            public static bool ColorEqual(ConsoleColor c, byte r, byte g, byte b)
            {
                var argb = GetARGB(c);
                return argb.Item1 == r && argb.Item2 == g && argb.Item3 == b;
            }

            public static int GetARGBInt(ConsoleColor cc)
            {
                var c = GetARGB(cc);
                var rgb = (int)c.Item3;
                rgb = (rgb << 8) + c.Item2;
                rgb = (rgb << 8) + c.Item1;
                return rgb;
            }

            public static (byte, byte, byte) GetARGB(ConsoleColor c)
            {
                switch (c)
                {
                    case ConsoleColor.Black: return new ValueTuple<byte, byte, byte>(0, 0, 0);
                    case ConsoleColor.DarkGray: return new ValueTuple<byte, byte, byte>(128, 128, 128);
                    case ConsoleColor.Gray: return new ValueTuple<byte, byte, byte>(192, 192, 192);
                    case ConsoleColor.White: return new ValueTuple<byte, byte, byte>(255, 255, 255);
                    case ConsoleColor.DarkBlue: return new ValueTuple<byte, byte, byte>(0, 0, 128);
                    case ConsoleColor.DarkGreen: return new ValueTuple<byte, byte, byte>(0, 128, 0);
                    case ConsoleColor.DarkRed: return new ValueTuple<byte, byte, byte>(128, 0, 0);
                    case ConsoleColor.DarkCyan: return new ValueTuple<byte, byte, byte>(0, 128, 128);
                    case ConsoleColor.DarkYellow: return new ValueTuple<byte, byte, byte>(128, 128, 0);
                    case ConsoleColor.DarkMagenta: return new ValueTuple<byte, byte, byte>(128, 0, 128);
                    case ConsoleColor.Blue: return new ValueTuple<byte, byte, byte>(0, 0, 255);
                    case ConsoleColor.Green: return new ValueTuple<byte, byte, byte>(0, 255, 0);
                    case ConsoleColor.Red: return new ValueTuple<byte, byte, byte>(255, 0, 0);
                    case ConsoleColor.Cyan: return new ValueTuple<byte, byte, byte>(0, 255, 255);
                    case ConsoleColor.Yellow: return new ValueTuple<byte, byte, byte>(255, 255, 0);
                    case ConsoleColor.Magenta: return new ValueTuple<byte, byte, byte>(255, 0, 255);
                }
                return (0, 0, 0);
            }

            public static (byte, byte, byte) Mix(ConsoleColor a, ConsoleColor b, double mixing)
            {
                var left = GetARGB(a);
                var right = GetARGB(b);
                return ((byte)(left.Item1 + (right.Item1 - left.Item1) * mixing).Round(),
                        (byte)(left.Item2 + (right.Item2 - left.Item2) * mixing).Round(),
                        (byte)(left.Item3 + (right.Item3 - left.Item3) * mixing).Round());
            }

            public static BaseComplexPixel[] GetBasePixels(Dictionary<char, double> context)
            {
                var lst = new List<BaseComplexPixel>(16 * 16 * context.Count);
                for (var i = 0; i < 16; i++)
                {
                    var current = new BaseComplexPixel();
                    current.Character = ' ';
                    current.Background = (ConsoleColor)i;
                    current.Foreground = ConsoleColor.Black;
                    var gc = GetARGB(current.Background);
                    current.R = gc.Item1;
                    current.G = gc.Item2;
                    current.B = gc.Item3;
                    lst.Add(current);
                }

                for (var i = 0; i < 16; i++)
                {
                    for (var j = 0; j < 16; j++)
                    {
                        if (i == j) continue;
                        foreach (var ch in context)
                        {
                            var current = new BaseComplexPixel();
                            current.Foreground = (ConsoleColor)i;
                            current.Background = (ConsoleColor)j;
                            current.Character = ch.Key;
                            var mx = Mix(current.Background, current.Foreground, ch.Value);
                            current.R = mx.Item1;
                            current.G = mx.Item2;
                            current.B = mx.Item3;
                            if (!(lst.Any(x => (x.R - current.R).Power(2) + (x.G - current.G).Power(2) + (x.B - current.B).Power(2) == 0))) lst.Add(current);
                        }
                    }
                }
                return lst.ToArray();
            }

            public static BaseComplexPixel[] Create(char[] chars) => GetBasePixels(chars.ToDictionary(x => x, x => (double)x.GetMixingMultiplier()));

            public static BaseComplexPixel[] Create(string str) => Create(str.ToCharArray());

            public static BaseComplexPixel[] DefaultPixels { get; set; }

            public static BaseComplexPixel Find(BaseComplexPixel[] colors, byte r, byte g, byte b)
            {
                var absi = 0;
                var abssum = (colors[0].R - r).Power(2) + (colors[0].G - g).Power(2) + (colors[0].B - b).Power(2);
                for (var i = 1; i < colors.Length; i++)
                {
                    var currentabssum = (colors[i].R - r).Power(2) + (colors[i].G - g).Power(2) + (colors[i].B - b).Power(2);
                    if (currentabssum < abssum)
                    {
                        absi = i;
                        abssum = currentabssum;
                        if (abssum < 2) return colors[i];
                    }
                }
                return colors[absi];
            }

            public static BaseComplexPixel[] ConsoleColors { get; set; }

            public static ConsoleColor FromARGB(byte r, byte g, byte b) => Find(ConsoleColors, r, g, b).Background;

            static Convertion()
            {
                ConsoleColors = Enumerable.Range(0, 16).Select(x => (ConsoleColor)x).ToArray().ConvertAll(x =>
                {
                    var bcp = new BaseComplexPixel();
                    var argb = GetARGB(x);
                    bcp.Background = bcp.Foreground = x;
                    bcp.R = argb.Item1;
                    bcp.G = argb.Item2;
                    bcp.B = argb.Item3;
                    return bcp;
                });
                var context = new Dictionary<char, double>();
                context.Add('.', 0.06);
                context.Add(':', 0.13);
                context.Add('%', 0.21);
                context.Add('t', 0.29);
                context.Add('░', 0.33);
                context.Add('8', 0.4);
                context.Add('#', 0.44);
                context.Add('▄', 0.5);
                context.Add('█', 0.66);
                DefaultPixels = GetBasePixels(context);
            }
        }

        public static void DebugWrite()
        {
            for (var i = 0; i < Convertion.DefaultPixels.Length; i++)
            {
                Console.BackgroundColor = Convertion.DefaultPixels[i].Background;
                Console.ForegroundColor = Convertion.DefaultPixels[i].Foreground;
                Console.Write(Convertion.DefaultPixels[i].Character);
            }
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public ConsoleColor? Background { get; set; }
        public ComplexPixel[] Pixels { get; set; }

        public bool Optimized => OptimizedPixels != null;
        public ComplexPixelLine[] OptimizedPixels { get; set; }

        public Complexive Optimize()
        {
            var lst = new List<ComplexPixelLine>();
            var cl = new ComplexPixelLine();

            if (Pixels.Length == 0)
            {
                OptimizedPixels = lst.ToArray();
                return this;
            }

            var lastx = Pixels[0].PositionLeft;
            var lasty = Pixels[0].PositionTop;

            cl.Pixel = Pixels[0];
            cl.Length = 1;
            for (var i = 1; i < Pixels.Length; i++)
            {
                var curx = Pixels[i].PositionLeft;
                var cury = Pixels[i].PositionTop;

                if (curx == lastx + 1 && cury == lasty && cl.Pixel.EqualsAsAbstract(Pixels[i])) cl.Length++;
                else
                {
                    lst.Add(cl);
                    cl = new ComplexPixelLine();
                    cl.Pixel = Pixels[i];
                }

                lastx = curx;
                lasty = cury;
            }
            lst.Add(cl);
            OptimizedPixels = lst.ToArray();

            return this;
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(Width);
            bw.Write(Height);
            if (!Background.HasValue) bw.Write(int.MaxValue);
            else bw.Write((int)Background.Value);
            bw.Write(Pixels.Length);
            for (var i = 0; i < Pixels.Length; i++) bw.Write((long)Pixels[i]);
        }

        public void WriteToFile(string filename)
        {
            using (var stream = File.Create(filename)) using (var bw = new BinaryWriter(stream)) Write(bw);
        }

        public Complexive Clone()
        {
            var rslt = new Complexive();

            rslt.Width = Width;
            rslt.Height = Height;
            rslt.Background = Background;
            rslt.Pixels = new ComplexPixel[Pixels.Length];
            Array.Copy(Pixels, rslt.Pixels, Pixels.Length);

            return rslt;
        }

/*        /// <summary>
        /// Обрезает картинку до заданных границ. Это метод, меняющий текущий объект.
        /// </summary>
        public Complexive Cut(int width, int height, IContainer container, Alignment alignment)
        {
            if (Width <= width && Height <= height) return this;

            var rect = new Rectangle(0, 0, Width, Height, container, alignment);
            var (left, top) = rect.GetCorner();

            return Cut(left, top, width, height);
        }

        class Cutting : IContainer
        {
            private Complexive cp;

            public Cutting(Complexive cmp)
            {
                cp = cmp;
            }

            public int Left => 0;

            public int Top => 0;

            public int Width => cp.Width;

            public int Height => cp.Height;
        }

        /// <summary>
        /// Обрезает картинку до заданных границ. Это метод, меняющий текущий объект.
        /// </summary>
        public Complexive Cut(int width, int height, Alignment alignment = Alignment.LeftWidth | Alignment.UpHeight) => Cut(width, height, new Cutting(this), alignment);

        public Complexive Cut(int left, int top, int width, int height, bool save_original_size = false)
        {
            Width = width;
            Height = height;
            Pixels = Pixels.Where(x => x.PositionLeft >= left && x.PositionLeft < left + width && x.PositionTop >= top && x.PositionTop < top + height && !(x.PositionLeft == left + width && x.PositionTop == top + height))
                .ToArray().ConvertAll(x => new ComplexPixel(x.PositionLeft - left, x.PositionTop - top, x.Character, x.ForegroundColor, x.BackgroundColor));
            return this;
        }*/

        public Complexive UpdatePixels(IEnumerable<ComplexPixel> pixels)
        {
            Pixels = pixels.ToArray();
            return this;
        }

        public static Complexive Read(BinaryReader br)
        {
            var complexive = new Complexive();
            complexive.Width = br.ReadInt32();
            complexive.Height = br.ReadInt32();
            var back = br.ReadInt32();
            if (back == int.MaxValue) complexive.Background = null;
            else complexive.Background = (ConsoleColor)back;
            var count = br.ReadInt32();
            complexive.Pixels = new ComplexPixel[count];
            var bytes = br.ReadBytes(count * 8);
            for (var i = 0; i < count; i++)
            {
                complexive.Pixels[i] = (ComplexPixel)BitConverter.ToInt64(bytes, i * 8);
            }
            return complexive;
        }

        public static Complexive FromBitmap(Bitmap b, Convertion.BaseComplexPixel[] convertion = null)
        {
            if (convertion == null) convertion = Convertion.DefaultPixels;

            var pixels = new List<ComplexPixel>();
            var locker = new object();

            var Result = new Complexive();
            var width = Result.Width = b.Width;
            var height = Result.Height = b.Height;

            var rect = new System.Drawing.Rectangle(0, 0, Result.Width, Result.Height);
            var bdata = b.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, b.PixelFormat);
            var ptr = bdata.Scan0;
            var bytes = Math.Abs(bdata.Stride) * Result.Height;
            var argb = new byte[bytes];
            var colors = new List<(Color, int, int)>();

            System.Runtime.InteropServices.Marshal.Copy(ptr, argb, 0, bytes);
            b.UnlockBits(bdata);

            var bga = true;

            if (b.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                Parallel.For(0, argb.Length / 4, i =>
                {
                    var xx = i % width;
                    var yy = i / width;
                    var currentcolor = Color.FromArgb(argb[i * 4 + 3], argb[i * 4 + 2], argb[i * 4 + 1], argb[i * 4]);
                    if (currentcolor.A == 0)
                    {
                        bga = false;
                        return;
                    }
                    lock (locker) colors.Add((currentcolor, xx, yy));
                });
            }
            else
            {
                for (var i = 0; i < b.Width; i++)
                    for (var j = 0; j < b.Height; j++)
                        colors.Add((b.GetPixel(i, j), i, j));
            }

            if (bga)
            {
                var c = Color.FromArgb(colors.Select(x => x.Item1).GroupBy(x => x.ToArgb()).MaxBy(x => x.Count()).Key);
                Result.Background = Convertion.FromARGB(c.R, c.G, c.B);
            }

            var background = Result.Background ?? default;

            Parallel.For(0, colors.Count - 1, i =>
            {
                if (bga && Convertion.ColorEqual(background, colors[i].Item1.R, colors[i].Item1.G, colors[i].Item1.B)) return;
                else
                {
                    var pix = Convertion.Find(convertion, colors[i].Item1.R, colors[i].Item1.G, colors[i].Item1.B)
                    .ToConcretePixel(colors[i].Item2, colors[i].Item3);
                    lock (locker) pixels.Add(pix);
                }
            });
            pixels.Sort();

            Result.Pixels = pixels.ToArray();
            return Result;
        }

        public static Complexive FromBitmap(string filename, Convertion.BaseComplexPixel[] convertion = null)
        {
            using (var b = new Bitmap(filename)) return FromBitmap(b, convertion);
        }

        public static Complexive FromFile(string filename)
        {
            using (var stream = File.OpenRead(filename))
            using (var br = new BinaryReader(stream)) return Read(br);
        }

        object ICloneable.Clone() => Clone();
    }
}
