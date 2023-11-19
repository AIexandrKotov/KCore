using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using KCore.Extensions;

namespace KCore.Graphics.Core
{
    public class Primitive
    {
        public static readonly Primitive Empty = new Primitive(new Line[0]);
        public static Primitive GetEmpty(int width, int height)
        {
            var prim = new Primitive();
            prim.Width = width;
            prim.Height = height;
            prim.Lines = new Line[0];
            return prim;
        }
        public class OffsetsConfigurationFile
        {
            public (int, int) this[string index]
            {
                get
                {
                    if (Offsets.ContainsKey(index)) return Offsets[index];
                    else return (0, 0);
                }
                set
                {
                    if (Offsets.ContainsKey(index)) Offsets[index] = value;
                    else Offsets.Add(index, value);
                }
            }

            internal Dictionary<string, (int, int)> Offsets { get; set; }

            public OffsetsConfigurationFile(Dictionary<string, (int, int)> dict)
            {
                Offsets = new Dictionary<string, (int, int)>(dict);
            }

            public OffsetsConfigurationFile(string path)
            {
                var s = File.ReadAllLines(path);
                Offsets = new Dictionary<string, (int, int)>();
                for (var i = 0; i < s.Length; i++)
                {
                    if (s[i].StartsWith("//") || string.IsNullOrWhiteSpace(s[i])) continue;

                    var splitter = s[i].IndexOf('=');
                    var tuple = s[i].Substring(splitter + 1, s[i].Length - splitter - 1).ToWords();
                    Offsets.Add(s[i].Substring(0, splitter), (tuple[0].ToInt32(), tuple[1].ToInt32()));
                }
            }

            public void SaveConfigurationFile()
            {
                var sb = new StringBuilder();
                foreach (var pair in Offsets)
                {
                    sb.Append($"{pair.Key}={pair.Value.Item1} {pair.Value.Item2}");
                    sb.AppendLine();
                }
            }
        }

        private static readonly Color white = Color.FromArgb(255, 255, 255);
        public Line[] Lines { get; set; }
        public (int, int) Offset { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Primitive[] Sever()
        {
            var prims = new List<Primitive>();
            var lns = new List<Line>();
            var last = 0;
            for (var i = 0; i < Lines.Length; i++)
            {
                var current = Lines[i].PositionTop;
                if (last != current)
                {
                    var currentprim = new Primitive(lns.ToArray());
                    currentprim.Width = Width;
                    currentprim.Height = Height;
                    prims.Add(currentprim);
                    lns.Clear();
                }
                lns.Add(Lines[i]);
                last = current;
            }
            if (lns.Count > 0) prims.Add(new Primitive(lns.ToArray()));
            return prims.ToArray();
        }

        internal Primitive() { }

        public Primitive(Line[] lines)
        {
            Lines = lines;
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(Offset.Item1);
            bw.Write(Offset.Item2);
            bw.Write(Width);
            bw.Write(Height);
            bw.Write(Lines.Length);
            for (var i = 0; i < Lines.Length; i++)
            {
                bw.Write(Lines[i].ToInt64());
            }
        }

        public void WriteToFile(string filename)
        {
            using (var stream = File.Create(filename)) using (var bw = new BinaryWriter(stream)) Write(bw);
        }

        public static Primitive Read(BinaryReader br)
        {
            var result = new Primitive();

            result.Offset = (br.ReadInt32(), br.ReadInt32());
            result.Width = br.ReadInt32();
            result.Height = br.ReadInt32();
            var lines = br.ReadInt32();
            result.Lines = new Line[lines];
            var bytes = br.ReadBytes(lines * 8);
            for (var i = 0; i < lines; i++)
                result.Lines[i] = new Line(BitConverter.ToInt64(bytes, i * 8));
            return result;
        }

        public static Primitive ReadFile(string filename)
        {
            using (var stream = File.OpenRead(filename)) using (var br = new BinaryReader(stream)) return Read(br);
        }

        internal static List<Line> ReadBitmap(Bitmap bitmap)
        {
            var list = new List<Line>(bitmap.Height * 2);
            var isline = false;
            var thisline = new Line();

            for (var j = 0; j < bitmap.Size.Height; j++)
            {
                if (isline)
                {
                    list.Add(thisline.Clone());
                    isline = false;
                }
                for (var i = 0; i < bitmap.Size.Width; i++)
                {
                    if (bitmap.GetPixel(i, j) == white)
                    {
                        if (isline)
                        {
                            list.Add(thisline.Clone());
                            isline = false;
                        }
                    }
                    else
                    {
                        if (isline)
                        {
                            thisline.Length += 1;
                        }
                        else
                        {
                            isline = true;
                            thisline.PositionLeft = (short)i;
                            thisline.PositionTop = (short)j;
                            thisline.Length = 1;
                        }
                    }
                }
            }
            if (isline) list.Add(thisline.Clone());
            return list;
        }

        internal static List<Line> ReadBitmap(Bitmap bitmap, int left, int top, int width, int height)
        {
            var list = new List<Line>(height * 2);
            var isline = false;
            var thisline = new Line();

            for (var j = top; j < top + height; j++)
            {
                if (isline)
                {
                    list.Add(thisline.Clone());
                    isline = false;
                }
                for (var i = left; i < left + width; i++)
                {
                    if (bitmap.GetPixel(i, j) == white)
                    {
                        if (isline)
                        {
                            list.Add(thisline.Clone());
                            isline = false;
                        }
                    }
                    else
                    {
                        if (isline)
                        {
                            thisline.Length += 1;
                        }
                        else
                        {
                            isline = true;
                            thisline.PositionLeft = (short)i;
                            thisline.PositionTop = (short)j;
                            thisline.Length = 1;
                        }
                    }
                }
            }
            if (isline) list.Add(thisline.Clone());
            return list;
        }

        public static Primitive CreateFromBitmap(Bitmap b, int left, int top, int width, int height, OffsetsConfigurationFile offsets, string nameinoffset)
        {
            var prim = new Primitive();
            prim.Offset = offsets[nameinoffset];
            prim.Width = b.Size.Width;
            prim.Height = b.Size.Height;
            prim.Lines = ReadBitmap(b, left, top, width, height).ToArray();
            return prim;
        }

        public static Primitive CreateFromBitmap(Bitmap b, int left, int top, int width, int height)
        {
            var prim = new Primitive();
            prim.Offset = (0, 0);
            prim.Width = b.Size.Width;
            prim.Height = b.Size.Height;
            prim.Lines = ReadBitmap(b, left, top, width, height).ToArray();
            return prim;
        }

        public static Primitive CreateFromBitmap(Bitmap b, int left, int top, int width, int height, int offsetwidth, int offsetheight)
        {
            var prim = new Primitive();
            prim.Offset = (offsetwidth, offsetheight);
            prim.Width = b.Size.Width;
            prim.Height = b.Size.Height;
            prim.Lines = ReadBitmap(b, left, top, width, height).ToArray();
            return prim;
        }

        public static Primitive[] CreateFromBitmap(string fname, int count, int width, int height, Func<int, (int, int)> startpose, OffsetsConfigurationFile offsets)
        {
            var result = new Primitive[count];
            using (var b = new Bitmap(fname))
            {
                for (var i = 0; i < count; i++)
                {
                    var prim = new Primitive();
                    prim.Offset = offsets[Path.GetFileNameWithoutExtension(fname) + count];
                    prim.Width = b.Size.Width;
                    prim.Height = b.Size.Height;
                    var pos = startpose(i);
                    prim.Lines = ReadBitmap(b, pos.Item1, pos.Item2, width, height).ToArray();
                    result[i] = prim;
                }
            }
            return result;
        }

        public static Primitive CreateFromBitmap(string fname)
        {
            return CreateFromBitmap(fname, 0, 0);
        }

        public static Primitive CreateFromBitmap(string fname, OffsetsConfigurationFile offsets)
        {
            using (var b = new Bitmap(fname))
            {
                var prim = new Primitive();
                prim.Offset = offsets[Path.GetFileNameWithoutExtension(fname)];
                prim.Width = b.Size.Width;
                prim.Height = b.Size.Height;
                prim.Lines = ReadBitmap(b).ToArray();
                return prim;
            }
        }

        public static Primitive CreateFromBitmap(string fname, int offsetwidth, int offsetheight)
        {
            using (var b = new Bitmap(fname))
            {
                var prim = new Primitive();
                prim.Offset = (offsetwidth, offsetheight);
                prim.Width = b.Size.Width;
                prim.Height = b.Size.Height;
                prim.Lines = ReadBitmap(b).ToArray();
                return prim;
            }
        }
    }
}
